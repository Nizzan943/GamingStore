using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Data;
using GamingStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamingStore.Controllers
{
    public class BaseController : Controller
    {
        protected readonly UserManager<User> UserManager;
        protected readonly GamingStoreContext Context;
        protected readonly RoleManager<IdentityRole> RoleManager;
        protected readonly SignInManager<User> SignInManager;

        public BaseController(UserManager<User> userManager, GamingStoreContext context, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
        {
            UserManager = userManager;
            Context = context;
            RoleManager = roleManager;
            SignInManager = signInManager;
        }

        protected Task<User> GetCurrentUserAsync() => UserManager.GetUserAsync(User);

        public Task<int> ItemsInCart => CountItemsInCart();

        protected async Task<int> CountItemsInCart()
        {
            User user = await GetCurrentUserAsync();

            if (user == null)
            {
                return 0;
            }

            var itemsInCart = 0;

            foreach (Cart itemInCart in Context.Cart.Where(c => c.UserId == user.Id))
            {
                itemsInCart += itemInCart.Quantity;
            }


            return itemsInCart;
        }
    }
}