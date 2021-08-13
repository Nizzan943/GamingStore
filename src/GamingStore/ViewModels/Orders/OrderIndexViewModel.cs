using System.Collections.Generic;
using GamingStore.Models;

namespace GamingStore.ViewModels.Orders
{
    public class OrderIndexViewModel : ViewModelBase
    {
        public List<Order> OrderList { get; set; }
    }
}
