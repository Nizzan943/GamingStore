using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using GamingStore.Contracts;

namespace GamingStore.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public List<Item> Items { get; set; }
        public User UserId { get; set; }
        public DateTime DateTime { get; set; }
        public string PaymentMethod { get; set; }
        public double Amount { get; set; }
        public Address BillingAddress { get; set; }
        public Address DeliveryAddress { get; set; }

    }
}
