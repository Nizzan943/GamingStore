using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamingStore.Data;
using GamingStore.Models;
using GamingStore.Models.Relationships;
using GamingStore.ViewModels;
using GamingStore.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace GamingStore.Controllers
{
    public class OrdersController : BaseController
    {
        private readonly GamingStoreContext _context;
        public OrdersController(UserManager<User> userManager, GamingStoreContext context, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
            : base(userManager, context, roleManager, signInManager)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                User User = await GetCurrentUserAsync();
                List<Cart> itemsInCart = await GetItemsInCart(User);
                bool inactiveItemsInCart = itemsInCart.Any(c => !c.Item.Active);

                if (inactiveItemsInCart)
                {
                    //"Some items in cart are no longer available");
                    return RedirectToAction("Index", "Carts");
                }

                const int defaultPaymentCost = 10;
                double itemsCost = itemsInCart.Aggregate<Cart, double>(0, (current, cart) => current + cart.Item.Price * cart.Quantity);

                if (itemsCost == 0)
                {
                    //"Your cart does no longer has items, please add items to cart before proceed to checkout");
                    return RedirectToAction("Index", "Carts");
                }

                var totalCost = itemsCost + defaultPaymentCost;
                User.Address ??= new Address();


                var itemsIdsInCartList = new List<int>();

                foreach (Cart itemInCart in itemsInCart)
                {
                    for (var i = 1; i <= itemInCart.Quantity; i++)
                    {
                        itemsIdsInCartList.Add(itemInCart.ItemId);
                    }
                }

                var viewModel = new CreateOrderViewModel
                {
                    Cart = itemsInCart,
                    User = User,
                    ShippingAddress = new ShippingAddress
                    {
                        City = User.Address.City,
                        Country = User.Address.Country,
                        FullName = User.UserName,
                        PostalCode = "00000",
                        Street = User.Address.Street,
                        Number = User.Address.Number
                    },

                    Payment = new Payment
                    {
                        ShippingCost = defaultPaymentCost,
                        ItemsCost = itemsCost,
                        Total = totalCost
                    },
                    ItemsInCart = await CountItemsInCart(),
                    ItemsIdsInCartList = itemsIdsInCartList
                };

                //"Please verify your items before placing the order");

                return View(viewModel);
            }
            catch (Exception e)
            {
                //"Your order could not be placed. Please contact support for help");
                //$"an order could not be created, e: {e}");

                return View(new CreateOrderViewModel() { ItemsInCart = await CountItemsInCart() });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            User User = await GetCurrentUserAsync();
            List<Cart> currentItemsInCart = await GetItemsInCart(User);

            var currentItemsIdsInCart = new List<int>();

            foreach (Cart cart in currentItemsInCart)
            {
                for (var i = 0; i < cart.Quantity; i++)
                {
                    currentItemsIdsInCart.Add(cart.ItemId);
                }
            }

            List<int> firstNotSecond = currentItemsIdsInCart.Except(model.ItemsIdsInCartList).ToList();
            List<int> secondNotFirst = model.ItemsIdsInCartList.Except(currentItemsIdsInCart).ToList();
            bool isEqual = !firstNotSecond.Any() && !secondNotFirst.Any();

            if (!isEqual)
            {
                //"Your cart items are different from your checkout items");
                return RedirectToAction("Index", "Carts");
            }

            double itemsCost = currentItemsInCart.Aggregate<Cart, double>(0, (current, cart) => current + cart.Item.Price * cart.Quantity);

            var order = new Order()
            {
                Payment = new Payment()
                {
                    PaymentMethod = PaymentMethod.CreditCard,
                    Paid = true,
                    ShippingCost = model.Payment.ShippingCost,
                    ItemsCost = itemsCost,
                    Total = itemsCost + model.Payment.ShippingCost
                },
                //PaymentId = model.Payment.Id,
                Store = await Context.Store.FirstOrDefaultAsync(s => s.Name == "Website"),
                //StoreId = 8, //website //need to change
                User = User,
                UserId = User.Id,
                State = OrderState.New,
                ShippingMethod = model.Payment.ShippingCost switch
                {
                    0 => ShippingMethod.Pickup,
                    10 => ShippingMethod.Standard,
                    45 => ShippingMethod.Express,
                    _ => ShippingMethod.Other
                },
                OrderDate = DateTime.Now,
                ShippingAddress = model.ShippingAddress,
                CreditCard = model.CreditCard
            };

            //handle order items
            foreach (Cart cartItem in currentItemsInCart)
            {
                order.OrderItems.Add(new OrderItem()
                {
                    ItemId = cartItem.ItemId,
                    Item = cartItem.Item,
                    ItemsCount = cartItem.Quantity,
                    Order = order,
                    OrderId = order.Id
                });
            }

            //add order to db
            Context.Add(order);
            await Context.SaveChangesAsync();

            return RedirectToAction("ThankYouIndex", new { id = order.Id });
        }

        private async Task<List<Cart>> GetItemsInCart(User User)
        {
            List<Cart> itemsInCart = await Context.Cart.Where(c => c.UserId == User.Id).ToListAsync();

            foreach (Cart cartItem in itemsInCart)
            {
                Item item = Context.Item.First(i => i.Id == cartItem.ItemId);
                cartItem.Item = item;
            }

            return itemsInCart;
        }

        public async Task<IActionResult> ThankYouIndex(string id)
        {
            User User = await GetCurrentUserAsync();

            List<Cart> itemsInCart = await GetItemsInCart(User);
            bool inactiveItemsInCart = itemsInCart.Any(c => !c.Item.Active);

            if (inactiveItemsInCart)
            {
                //"Some items in cart are no longer available");
                return RedirectToAction("Index", "Carts");
            }

            double itemsCost = itemsInCart.Aggregate<Cart, double>(0, (current, cart) => current + cart.Item.Price * cart.Quantity);

            if (itemsCost == 0)
            {
                //"Your cart does no longer has items, please add items to cart before proceed to checkout");
                return RedirectToAction("Index", "Carts");
            }

            Order order = null;
            var items = await ClearCartAsync(User);

            try
            {
                order = Context.Order.Include(o => o.User).First(o => o.Id == id);
            }
            catch
            {
                //ignored
            }

            if (order == null)
            {
                return View();
            }

            var viewModel = new OrderPlacedViewModel
            {
                ItemsCount = items,
                User = User,
                ShippingAddress = order.ShippingAddress,
                ItemsInCart = await CountItemsInCart(),
                OrderId = id
            };

            return View(viewModel);
        }

        private async Task<int> ClearCartAsync(User User)
        {
            IQueryable<Cart> carts = Context.Cart.Where(c => c.UserId == User.Id);
            var items = 0;

            foreach (Cart itemInCart in carts)
            {
                items += itemInCart.Quantity;
            }

            Context.Cart.RemoveRange(carts);
            await Context.SaveChangesAsync();
            return items;
        }


        // GET: Orders/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Order order = await Context.Order
                .Include(x => x.OrderItems)
                .ThenInclude(y => y.Item)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            order.User = await GetCurrentUserAsync();
            order.Payment = await Context.Payment.FirstOrDefaultAsync(payment => payment.Id == order.PaymentId);

            var viewModel = new OrderDetailsViewModel()
            {
                ItemsInCart = await CountItemsInCart(),
                Order = order
            };

            return View(viewModel);
        }

        // GET: Items/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Order order = await Context.Order.Include(o => o.Payment).FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new EditOrdersViewModel
            {
                User = await Context.Users.Where(User => !string.IsNullOrWhiteSpace(User.UserName))
                    .Distinct().ToListAsync(),
                Order = order,
                ItemsInCart = await CountItemsInCart(),
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<IActionResult> Edit(Order order)
        {
            if (!await OrderExists(order.Id))
            {
                //"You cannot edit product that is no longer exists");
                return RedirectToAction("ListOrders", "Administration");
            }


            var currentUser = await GetCurrentUserAsync();

            IList<string> userRoles = await UserManager.GetRolesAsync(currentUser);

            if (!userRoles.Any(r => r.Equals("Admin")))
            {
               //"Your changes were not saved.\n You do not have the right permissions to edit orders.");
                return RedirectToAction("ListOrders", "Administration");
            }

            try
            {
                Order orderOnDb = await Context.Order.Include(o => o.Payment).FirstOrDefaultAsync(o => o.Id == order.Id);
                orderOnDb.Payment.Paid = order.Payment.Paid;
                orderOnDb.Payment.PaymentMethod = order.Payment.PaymentMethod;
                orderOnDb.Payment.ShippingCost = order.Payment.ShippingCost;
                orderOnDb.Payment.ItemsCost = order.Payment.ItemsCost;
                orderOnDb.Payment.Total = order.Payment.Total;
                orderOnDb.Payment.Notes = order.Payment.Notes;
                orderOnDb.State = order.State;

                /*
                if (order.Payment.RefundAmount > order.Payment.Total)
                {
                    orderOnDb.Payment.RefundAmount = order.Payment.Total;
                }
                else
                {
                    orderOnDb.Payment.RefundAmount = order.Payment.RefundAmount;
                }

                */
                Context.Update(orderOnDb);
                await Context.SaveChangesAsync();
                //"Order has been updated";
            }
            catch (Exception e)
            {
                //"Order could not be updated");
               //$"Order# '{order.Id}' could not be updated, ex: {e}");
            }

            return RedirectToAction("ListOrders", "Administration");
        }

        private async Task<bool> OrderExists(string id)
        {
            return await Context.Order.AnyAsync(e => e.Id == id);
        }
    }
}