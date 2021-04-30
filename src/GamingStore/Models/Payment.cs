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
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string PaymentId { get; set; }

        public string OrderForeignKey { get; set; }

        [Required]
        [Range(0, 99999)]
        [Display(Name = "Total Cost")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public double Total { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public string RefundAmount { get; set; }

        public bool Paid { get; set; } = true;
    }
}
