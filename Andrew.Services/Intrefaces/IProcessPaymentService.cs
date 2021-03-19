using Andrew.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Andrew.Services.Intrefaces
{
    public interface IProcessPaymentService
    {
        Task<ProcessPaymentViewModel> ProcessPayment(ProcessPaymentViewModel paymentViewModel);
    }
}
