using Andrew.Context;
using Andrew.DataModels;
using Andrew.GenericRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Andrew.UnitOfWork
{
    public class UnitOfWork
    {
        private readonly EFDBContext _context;

        public UnitOfWork(EFDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private GenericRepository<UsersCreditCardDetailModel> usersCreditCardDetailModelRepository;
        public GenericRepository<UsersCreditCardDetailModel> UsersCreditCardDetailModelRepository
        {
            get
            {
                if (this.usersCreditCardDetailModelRepository == null)
                    this.usersCreditCardDetailModelRepository = new GenericRepository<UsersCreditCardDetailModel>(_context);
                return usersCreditCardDetailModelRepository;
            }
        }

        private GenericRepository<PaymentDetailModel> paymentDetailModelRepository;
        public GenericRepository<PaymentDetailModel> PaymentDetailModelRepository
        {
            get
            {
                if (this.paymentDetailModelRepository == null)
                    this.paymentDetailModelRepository = new GenericRepository<PaymentDetailModel>(_context);
                return paymentDetailModelRepository;
            }
        }
    }
}
