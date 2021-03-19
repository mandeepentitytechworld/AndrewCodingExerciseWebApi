using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Andrew.Models
{
    public class ProcessPaymentViewModel
    {
        public Int64 CartId { get; set; }
        public Int64 UserId { get; set; }

        #region CC detail
        [Required]
        public string CreditCardNumber { get; set; }
        [Required]
        public string CardHolderName { get; set; }
        [Required]
        public string ExpiryDate { get; set; }
        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "The field CVV must be a three digit number.")]
        [MaxLength(3, ErrorMessage = "The field CVV must be a three digit number.")]
        public string CVV { get; set; }
        #endregion CC detail

        #region Payment detail
        [Required]
        public decimal Amount { get; set; }
        public int PaymentGatewayId { get; set; }
        public string PaymentGatewayName { get; set; }
        #endregion Payment detail

        public MessageViewModel messageViewModel { get; set; }
    }
}
