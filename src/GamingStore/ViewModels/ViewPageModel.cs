using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Data;
using GamingStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GamingStore.ViewModels
{
    public class ViewPageModel : PageModel
    {
        private readonly GamingStoreContext _context;

        public ViewPageModel(GamingStoreContext context)
        {
            _context = context;
        }

        public int? ItemsInCart { get; set; } = null;

        protected async Task<int> CountItemsInCart(User user)
        {
            if (user == null)
            {
                return 0;
            }

            var itemsInCart = 0;

            foreach (Models.Cart itemInCart in _context.Cart.Where(c => c.UserId == user.Id))
            {
                itemsInCart += itemInCart.Quantity;
            }

            return itemsInCart;
        }
    }
}
