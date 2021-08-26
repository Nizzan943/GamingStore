using System.ComponentModel.DataAnnotations;

namespace GamingStore.Contracts
{
    public class CreditCard
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Owner Name")]
        [StringLength(50)]
        [RegularExpression(@"[a-zA-Z]{2,}$")]
        public string OwnerName { get; set; }

        [Required]
        [DataType(DataType.CreditCard)]
        [Display(Name = "Credit Card Number")]
        [Range(100000000000, 9999999999999999999, ErrorMessage = "Credit card number is not valid")]
        public string Number { get; set; }

        [Required]
        [Range(00, 12, ErrorMessage = "Expiration month is not valid")]
        [DataType(DataType.Currency)]
        [Display(Name = "Expiration Month")]
        public string ExpirationMonth { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Expiration Year")]
        [Range(2020, 2034, ErrorMessage = "Expiration month is not valid")]
        public string ExpirationYear { get; set; }

        [Required]
        [Range(000, 999, ErrorMessage = "CVV is not valid")]
        [DataType(DataType.Currency)]
        [Display(Name = "CVV (Code)")]
        public string CVV { get; set; }
    }
}
