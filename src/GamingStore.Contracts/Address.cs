using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GamingStore.Contracts
{
    public class Address
    {
        [Required]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Please enter a valid Street.")]
        public string? Street { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Range(1, 100000, ErrorMessage = "Please enter a valid Street Number.")]
        public int? Number { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Please enter a valid City.")]
        public string? City { get; set; }

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
