using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Contracts;

namespace GamingStore.Models
{
    public class Payment
    {
        public Payment()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string OrderForeignKey { get; set; }

        [Required]
        [Range(0, 99999)]
        [Display(Name = "Items Cost")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public double ItemsCost { get; set; }


        [Required]
        [Range(0, 99999)]
        [Display(Name = "Shipping Cost")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public int ShippingCost { get; set; } = 0;

        [Required]
        [Range(0, 99999)]
        [Display(Name = "Total Cost")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public double Total { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public double RefundAmount { get; set; }
        public string Notes { get; set; }
        public bool Paid { get; set; } = true;
    }
}
