﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GamingStore.Data;
using GamingStore.Models;
using GamingStore.ViewModels;
using GamingStore.ViewModels.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GamingStore.Controllers
{
    [Route("cart")]
    public class CartsController : BaseController
    {
        public CartsController(UserManager<User> userManager, GamingStoreContext context, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
            : base(userManager, context, roleManager, signInManager)
        {
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                User customer = await GetCurrentUserAsync();
                IQueryable<Cart> itemsInCart = Context.Cart.Where(c => c.UserId == customer.Id);

                foreach (Cart cartItem in itemsInCart)
                {
                    Item item = Context.Item.First(i => i.Id == cartItem.ItemId);
                    cartItem.Item = item;
                }

                var itemsPrice = 0.00;

                foreach (Cart cart in itemsInCart)
                {
                    itemsPrice += cart.Item.Price * cart.Quantity;
                }

                var viewModel = new CartViewModel
                {
                    Carts = await itemsInCart.ToListAsync(),
                    Payment = new Payment
                    {
                        ItemsCost = itemsPrice
                    },
                    ItemsInCart = await CountItemsInCart()
                };

                return View(viewModel);
            }
            catch
            {
                //no items in cart
                return View(new CartViewModel { ItemsInCart = 0 });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete()
        {
            User customer = await GetCurrentUserAsync();
            IQueryable<Cart> cart = Context.Cart.Where(c => c.UserId == customer.Id);
            Context.Cart.RemoveRange(cart);
            await Context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}