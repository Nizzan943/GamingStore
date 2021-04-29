using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Contracts;

namespace GamingStore.Models
{
    public class Payment
    {
        [Key]
        public string PaymentId { get; set; }

        public double Total { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public string RefundAmount { get; set; }

        public bool Paid { get; set; } = true;
    }
}
