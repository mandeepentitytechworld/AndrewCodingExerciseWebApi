using Andrew.Models;
using Andrew.Services.Intrefaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AndrewCodingExerciseWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IProcessPaymentService _processPaymentService;
        public PaymentController(IProcessPaymentService processPaymentService)
        {
            _processPaymentService = processPaymentService;
        }

        //[HttpGet]
        //public string Get()
        //{
        //    return "Hello World!";
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentViewModel"></param>
        /// <returns></returns>
        [HttpPost("ProcessPayment")]
        public async Task<ActionResult<ProcessPaymentViewModel>> ProcessPayment(ProcessPaymentViewModel paymentViewModel)
        {
            var result = await _processPaymentService.ProcessPayment(paymentViewModel);
            return result;
        }
    }
}