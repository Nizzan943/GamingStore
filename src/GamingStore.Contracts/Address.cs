using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GamingStore.Contracts
{
    public class Address
    {
        [Required]
        [Display(Name = "Full Name")]
        [DataType(DataType.Text)]
        public string FullName { get; set; }

        [DataType(DataType.Text)]
        public string Country { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "*City is not valid")]
        public string City { get; set; }
        
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Address")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Address is not valid")]
        public string Address1 { get; set; }
        
        [Required]
        [DataType(DataType.PostalCode)]
        [Display(Name = "Postal Code")]
        [Range(00000, 9999999, ErrorMessage = "Postal code is not valid")]
        public string PostalCode { get; set; }
        
        public string DeliveryNotes { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(13, MinimumLength = 9, ErrorMessage = "*Phone is not valid")]
        public string PhoneNumber { get; set; }
        
    }

}
