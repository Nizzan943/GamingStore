using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GamingStore.Contracts;
using GamingStore.Models;

namespace GamingStore.ViewModels.Orders
{
    public class CreateOrderViewModel : ViewModelBase
    {
        public int OrderId { get; set; }

        [Required] 
        public string CustomerId { get; set; }
        
        public User User { get; set; }
        
        [Required]
        public int StoreId { get; set; }
        
        public DateTime OrderDate { get; set; }
        
        public OrderState State { get; set; }

        public Address ShippingAddress { get; set; }
        
        public ShippingMethod ShippingMethod { get; set; }

        public CreditCard CreditCard { get; set; }

        public List<Models.Cart> Cart { get; set; }

        [Required]
        public Payment Payment { get; set; }

        public List<int> ItemsIdsInCartList { get; set; }
    }
}