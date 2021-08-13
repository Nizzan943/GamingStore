using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingStore.Contracts
{
    public class ShippingAddress
    {
        [Required]
        [Display(Name = "Full Name")]
        [DataType(DataType.Text)]
        public string? FullName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Street")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Address is not valid")]
        public string? Street { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "*City is not valid")]
        public string? City { get; set; }

        [Required]
        [DataType(DataType.PostalCode)]
        [Display(Name = "Postal Code")]
        [Range(00000, 9999999, ErrorMessage = "Postal code is not valid")]
        public string? PostalCode { get; set; }

        [DataType(DataType.Text)]
        public string? Country { get; set; } = "Israel";

        public override string ToString()
        {
            // shows values only if they aren't null.
            List<string?> values = typeof(Address).GetProperties().Select(prop => prop.GetValue(this, null))
                .Where(val => val != null).Select(val => val?.ToString()).Where(str => !string.IsNullOrEmpty(str))
                .ToList();

            return string.Join(", ", values);
        }
    }
}

