using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andrew.DataModels
{
    [Table("UsersCCDetail")]
    public class UsersCreditCardDetailModel
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int64 Id { get; set; }
        [Required]
        public string CreditCardNumber { get; set; }
        [Required]
        public string CardHolderName { get; set; }
        [Required]
        public string ExpiryDate { get; set; }
        public Int64 UserId { get; set; } //will be foreign key and required in actual but not here
    }

    [Table("PaymentDetail")]
    public  class PaymentDetailModel
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int64 Id { get; set; }
        [Required]
        public UsersCreditCardDetailModel CCUsed { get; set; }
        [Required]
        public DateTime PaymentDate { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string PaymentStatus { get; set; }
        [Required]
        public string PaymentGateway { get; set; }
        public Int64 CartId { get; set; } //will be foreign key and required in actual but not here
    }
}
