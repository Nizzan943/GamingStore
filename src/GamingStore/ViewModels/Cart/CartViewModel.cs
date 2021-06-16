using System.Collections.Generic;
using GamingStore.Models;

namespace GamingStore.ViewModels.Cart
{
    public class CartViewModel : ViewModelBase
    {
        public List<Models.Cart> Carts { get; set; }

        public Payment Payment { get; set; }
    }
}