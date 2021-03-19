using Andrew.Context;
using Andrew.DataModels;
using Andrew.Models;
using Andrew.Services.Intrefaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andrew.Services.Services
{
    public class ProcessPaymentService : IProcessPaymentService
    {
        private Andrew.UnitOfWork.UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProcessPaymentService(EFDBContext context, IMapper mapper)
        {
            _unitOfWork = new UnitOfWork.UnitOfWork(context);
            _mapper = mapper;
        }

        public async Task<ProcessPaymentViewModel> ProcessPayment(ProcessPaymentViewModel paymentViewModel)
        {
            paymentViewModel.messageViewModel = new MessageViewModel();
            paymentViewModel.messageViewModel.Status = true;

            if (paymentViewModel.Amount < 0) //The request is invalid: 400 bad request
            {
                paymentViewModel.messageViewModel.Status = false;
                paymentViewModel.messageViewModel.Message = Extentions.GetDescription(EnumClass.PaymentResponse.InvalidRequest);
                return paymentViewModel;
            }

            try
            {
                #region local variables

                string paymentGateway = string.Empty;
                int retryPayment = 0;
                string paymentGatewayResponse = Extentions.GetDescription(EnumClass.PaymentResponse.Processed);
                PaymentDetailModel paymentDetailModel = new PaymentDetailModel();
                PaymentDetailModel _paymentDetailModel = new PaymentDetailModel();
                UsersCreditCardDetailModel usersCreditCardDetailModel = new UsersCreditCardDetailModel();
                bool isCardValid = true;


                #endregion local variables

                #region choose payment gateway

                if (paymentViewModel.Amount <= 20) //If the amount to be paid is less than £20, use ICheapPaymentGateway.
                {
                    paymentViewModel.PaymentGatewayName = Extentions.GetDescription(EnumClass.PaymentGateways.CheapPaymentGateway);
                    paymentViewModel.PaymentGatewayId = (int)EnumClass.PaymentGateways.CheapPaymentGateway;
                }
                else if (paymentViewModel.Amount > 20 && paymentViewModel.Amount <= 500) //If the amount to be paid is £21 - 500, use IExpensivePaymentGateway
                {
                    paymentViewModel.PaymentGatewayName = Extentions.GetDescription(EnumClass.PaymentGateways.ExpensivePaymentGateway);
                    paymentViewModel.PaymentGatewayId = (int)EnumClass.PaymentGateways.ExpensivePaymentGateway;

                    //use IExpensivePaymentGateway if available. Otherwise, retry only once with ICheapPaymentGateway.
                    if (!ValidatePaymentGateway(paymentViewModel.PaymentGatewayName))
                    {
                        paymentViewModel.PaymentGatewayName = Extentions.GetDescription(EnumClass.PaymentGateways.CheapPaymentGateway);

                        if (!ValidatePaymentGateway(paymentViewModel.PaymentGatewayName)) //Any error: 500 internal server error
                        {
                            paymentViewModel.messageViewModel.Status = false;
                            paymentViewModel.messageViewModel.Message = Extentions.GetDescription(EnumClass.PaymentResponse.Failed);
                            return paymentViewModel;
                        }
                    }
                }
                else //If the amount is > £500, try only PremiumPaymentService
                {
                    paymentViewModel.PaymentGatewayName = Extentions.GetDescription(EnumClass.PaymentGateways.PremiumPaymentGateway);
                    paymentViewModel.PaymentGatewayId = (int)EnumClass.PaymentGateways.PremiumPaymentGateway;
                }

                #endregion choose payment gateway

                #region Validate & Save new CC detail

                //validate card
                isCardValid = ValidateCreditCard(paymentViewModel);

                //return here if CC detail is not valid
                if (!isCardValid) //400 bad request : Invaild Card Detail
                {
                    paymentViewModel.messageViewModel.Status = isCardValid;
                    paymentViewModel.messageViewModel.Message = Extentions.GetDescription(EnumClass.PaymentResponse.InvaildCardDetail);
                    return paymentViewModel;
                }

                //save card detail into database
                usersCreditCardDetailModel = await SaveUserCCDetail(paymentViewModel);
                if (usersCreditCardDetailModel == null)
                {
                    paymentViewModel.messageViewModel.Status = false;
                    paymentViewModel.messageViewModel.Message = Extentions.GetDescription(EnumClass.PaymentResponse.Failed);
                    return paymentViewModel;
                }

                #endregion Save/Update CC detail

                #region make payment

                //get payment gateway response
                paymentGatewayResponse = await MakePayment(paymentViewModel);

                #endregion make payment

                #region Save payment detail

                paymentDetailModel.Amount = paymentViewModel.Amount;
                paymentDetailModel.CCUsed = usersCreditCardDetailModel;
                paymentDetailModel.PaymentDate = DateTime.UtcNow;
                paymentDetailModel.CartId = paymentViewModel.CartId;
                paymentDetailModel.PaymentGateway = paymentViewModel.PaymentGatewayName;
                paymentDetailModel.PaymentStatus = paymentGatewayResponse;

                await _unitOfWork.PaymentDetailModelRepository.InsertAsync(paymentDetailModel);

                _paymentDetailModel = _mapper.Map<PaymentDetailModel>(paymentDetailModel);

                #endregion Save payment detail

                #region check payment status and update entity as completed

                //If the amount is > £500, try only PremiumPaymentService and retry up to 3 times in case payment does not get processed
                while (paymentViewModel.Amount > 500 &&
                    paymentGatewayResponse != Extentions.GetDescription(EnumClass.PaymentResponse.Processed))
                {
                    if (ValidatePaymentGateway(paymentViewModel.PaymentGatewayName))
                        paymentGatewayResponse = await MakePayment(paymentViewModel);

                    if (retryPayment == 3 || paymentGatewayResponse == Extentions.GetDescription(EnumClass.PaymentResponse.Processed))
                        break;

                    retryPayment = retryPayment + 1;
                }

                //Store/update the payment and payment state entities created previously once the processing is completed
                if (paymentDetailModel.PaymentStatus == Extentions.GetDescription(EnumClass.PaymentResponse.Processed))
                {
                    //Use of eager loading
                    var paymentDetail = _unitOfWork.PaymentDetailModelRepository.FindAllByQuery(c => c.Id == paymentDetailModel.Id)
                                                                                .Include(c => c.CCUsed)
                                                                                .FirstOrDefault();

                    paymentDetail.PaymentStatus = Extentions.GetDescription(EnumClass.PaymentResponse.Completed);

                    await _unitOfWork.PaymentDetailModelRepository.UpdateAsync(paymentDetail);
                }

                #endregion check payment status

                paymentViewModel.messageViewModel.Id = paymentDetailModel.Id;
                paymentViewModel.messageViewModel.Message = paymentGatewayResponse; //200 Ok
            }
            catch (Exception ex) //500 internal server error
            {
                paymentViewModel.messageViewModel.Status = false;
                paymentViewModel.messageViewModel.Message = Extentions.GetDescription(EnumClass.PaymentResponse.Failed);
            }
            return paymentViewModel;
        }

        /// <summary>
        /// make payment at payment gateway api
        /// </summary>
        /// <param name="processPaymentViewModel"></param>
        /// <returns></returns>
        private async Task<string> MakePayment(ProcessPaymentViewModel paymentViewModel)
        {

            //TO DO: make payment using payment gateway API 
            switch (paymentViewModel.PaymentGatewayId)
            {
                case 1: //CheapPaymentGateway
                    break;
                case 2://ExpensivePaymentGateway
                    break;
                case 3://PremiumPaymentGateway
                    break;
                default:
                    break;
            }

            return Extentions.GetDescription(EnumClass.PaymentResponse.Processed);
        }

        private bool ValidatePaymentGateway(string gateway)
        {
            return true;
        }

        private bool ValidateCreditCard(ProcessPaymentViewModel paymentViewModel)
        {
            try
            {
                //TO DO: check cc is valid or not
                //TO DO: if cc is not valid using API than set isCardValid = false
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<UsersCreditCardDetailModel> SaveUserCCDetail(ProcessPaymentViewModel paymentViewModel)
        {
            UsersCreditCardDetailModel usersCreditCardDetailModel = new UsersCreditCardDetailModel();
            try
            {
                usersCreditCardDetailModel = _unitOfWork.UsersCreditCardDetailModelRepository.FindAllBy(c => c.CreditCardNumber == paymentViewModel.CreditCardNumber).FirstOrDefault();
                if (usersCreditCardDetailModel == null)
                {
                    usersCreditCardDetailModel = new UsersCreditCardDetailModel();
                    usersCreditCardDetailModel.CardHolderName = paymentViewModel.CardHolderName;
                    usersCreditCardDetailModel.CreditCardNumber = paymentViewModel.CreditCardNumber;
                    usersCreditCardDetailModel.ExpiryDate = paymentViewModel.ExpiryDate;
                    usersCreditCardDetailModel.UserId = paymentViewModel.UserId;

                    await _unitOfWork.UsersCreditCardDetailModelRepository.InsertAsync(usersCreditCardDetailModel);
                }
            }
            catch (Exception)
            {

            }
            return usersCreditCardDetailModel;
        }
    }
}
