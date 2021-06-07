using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using GamingStore.Contracts;

namespace GamingStore.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public List<Item> Items { get; set; }
        [Required,DisplayName("User ID")]

        public int UserId { get; set; }

        public User User { get; set; }

        [Required, DisplayName("Order Date")]
        public DateTime OrderDate { get; set; }

        public OrderState State { get; set; }

        public string PaymentId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        
        public double Amount { get; set; }
        
        [NotMapped]
        public Address Address { get; set; }

    }
}
