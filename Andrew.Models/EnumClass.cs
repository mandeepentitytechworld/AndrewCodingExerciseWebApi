using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Andrew.Models
{
    public class EnumClass
    {
        public enum PaymentResponse
        {
            [Description("Pending")]
            Pending,
            [Description("200 Ok")]
            Processed,
            [Description("Completed")]
            Completed,
            [Description("500 internal server error")]
            Failed,
            [Description("400 bad request : Invaild Card Detail")]
            InvaildCardDetail,
            [Description("400 bad request")]
            InvalidRequest
        }

        public enum PaymentGateways
        {
            [Description("ExpensivePaymentGateway")]
            ExpensivePaymentGateway = 1,
            [Description("CheapPaymentGateway")]
            CheapPaymentGateway = 2,
            [Description("PremiumPaymentGateway")]
            PremiumPaymentGateway = 3
        }
    }
}
