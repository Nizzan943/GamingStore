using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using GamingStore.Contracts;
using GamingStore.Models.Relationships;

namespace GamingStore.Models
{
    public class Order
    {

        public Order()
        {
            OrderItems = new List<OrderItem>();

            Id = Guid.NewGuid().ToString();
        }

        [Key]
        public string Id { get; set; }

        [Required,DisplayName("User ID")]
        public int UserId { get; set; }

        public User User { get; set; }


        //[Required]
        //public int StoreId { get; set; }

        //public Store Store { get; set; }

        [Required, DisplayName("Order Date")]
        public DateTime OrderDate { get; set; }

        [Required]
        public OrderState State { get; set; }

        [NotMapped]
        public CreditCard CreditCard { get; set; }

        [DisplayName("Shipping Address")]
        public Address ShippingAddress { get; set; }

        public string PaymentId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
