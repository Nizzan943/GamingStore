using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.ViewModels;
using GamingStore.ViewModels.Administration;
using GamingStore.Models;
using GamingStore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using GamingStore.Contracts;
using System.IO;
using GamingStore.Models.Relationships;

namespace GamingStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : BaseController
    {

        private readonly GamingStoreContext _context;

        public AdministrationController(UserManager<User> userManager, GamingStoreContext context, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
            : base(userManager, context, roleManager, signInManager)
        {
            _context = context;
        }

        // GET: Administrator
        public async Task<ActionResult> Index()
        {
            var revenue = await CalcRevenue();
            var ordersNumber = await _context.Order.CountAsync();
            var itemsNumber = await _context.Item.CountAsync();
            var clientsNumber = await _context.User.CountAsync();

            revenue = Math.Round(revenue, 2);
            CreateMonthlyRevenueBarChartData(await _context.Order.Include(o => o.Payment).Include(o => o.Store).ToListAsync());
            //CreateRevenueByCategoryPieChartData(await _context.Order.Include(o => o.OrderItems).ThenInclude(oi => oi.Item).ToListAsync());

            List<Order> orders = await _context.Order
                .Include(order => order.User)
                .Include(order => order.Payment)
                .Include(order => order.Store)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();

            var viewModel = new IndexViewModel()
            {
                Customers = _context.User,
                Items = _context.Item,
                Stores = _context.Store,
                Orders = orders,
                ItemsInCart = await CountItemsInCart(),
                WidgetsValues = new Dictionary<string, double>()
                {
                    {"Revenue", revenue}, {"Orders", ordersNumber}, {"Items", itemsNumber}, {"Clients", clientsNumber}
                }
            };

            return View(viewModel);
        }

        private void CreateRevenueByCategoryPieChartData(List<Order> orders)
        {
            var purchaseByCategoryList = new List<PieChartFormat>();

            foreach (var order in orders)
            {
                foreach (OrderItem orderItem in order.OrderItems)
                {
                    var categoryName = orderItem.Item.Category.Name;
                    var itemsCost = orderItem.ItemsCount * orderItem.Item.Price;
                    if (purchaseByCategoryList.Any(d => d.Name == categoryName))
                    {
                        PieChartFormat pieChartFormat = purchaseByCategoryList.FirstOrDefault(d => d.Name == categoryName);
                        if (pieChartFormat != null)
                        {
                            pieChartFormat.Value += itemsCost;
                        }
                    }
                    else
                    {
                        purchaseByCategoryList.Add(new PieChartFormat()
                        {
                            Name = categoryName,
                            Value = itemsCost
                        });
                    }
                }
            }

            purchaseByCategoryList.Sort((a, b) => a.Value.CompareTo(b.Value));
            var serializeObject = JsonConvert.SerializeObject(purchaseByCategoryList, Formatting.Indented);

            //write string to file
            string pieChartDataPath = "data\\RevenueByCategoryPieChartData.json";
            var fileDir = $@"{Directory.GetCurrentDirectory()}\wwwroot\{pieChartDataPath}";
            System.IO.File.WriteAllText(fileDir, serializeObject);
        }

        private static void CreateMonthlyRevenueBarChartData(List<Order> orders)
        {
            orders.Sort((x, y) => x.OrderDate.CompareTo(y.OrderDate));

            var groupByCheck = from order in orders
                               group order by order.OrderDate.Date.ToString("Y")
                into dateGroup
                               select new BarChartFormat()
                               {
                                   Date = dateGroup.Key,
                                   Value = dateGroup.Sum(o => o.Payment.ItemsCost)
                               };

            var serializedGroupBy = JsonConvert.SerializeObject(groupByCheck, Formatting.Indented);

            //write string to file
            string barChartDataPath = "data\\MonthlyRevenueBarChartData.json";
            var fileDir = $@"{Directory.GetCurrentDirectory()}\wwwroot\{barChartDataPath}";
            System.IO.File.WriteAllText(fileDir, serializedGroupBy);
        }

        private void createStoresRevenueGraphData(List<Order> orders)
        {
            var orderMonthlyList = new List<PieChartFormat>();

            foreach (var order in orders)
            {
                var storeName = order.Store.Name.Replace("Store", "");
                var itemsCost = order.Payment.ItemsCost;

                if (orderMonthlyList.Any(d => d.Name == storeName))
                {
                    PieChartFormat pieChartFormat = orderMonthlyList.FirstOrDefault(d => d.Name == storeName);
                    if (pieChartFormat != null)
                    {
                        pieChartFormat.Value += itemsCost;
                    }
                }
                else
                {
                    orderMonthlyList.Add(new PieChartFormat()
                    {
                        Name = storeName,
                        Value = itemsCost
                    });
                }
            }

            orderMonthlyList.Sort((a, b) => a.Value.CompareTo(b.Value));

            var serializeObject = JsonConvert.SerializeObject(orderMonthlyList, Formatting.Indented);

            //write string to file
            string pieChartDataPath = "data\\StoresRevenuePieChart.json";
            var fileDir = $@"{Directory.GetCurrentDirectory()}\wwwroot\{pieChartDataPath}";
            System.IO.File.WriteAllText(fileDir, serializeObject);
        }



        private async Task<double> CalcRevenue()
        {
            var orders = await _context.Order.ToListAsync();
            double sum = 0.0;
            foreach (Order order in orders)
            {
                var payment = await _context.Payment.FindAsync(order.PaymentId);
                sum += payment.Total;
            }
            return sum;
        }


        [HttpGet]
        public async Task<IActionResult> ListItems()
        {
            List<Item> items = await _context.Item.ToListAsync();

            var viewModel = new ListItemsViewModel()
            {
                Items = items
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ListCategories()
        {
            List<Category> categories = await _context.Category.ToListAsync();

            var viewModel = new ListCategoriesViewModel()
            {
                Categories = categories
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ListStores()
        {
            List<Store> stores = await _context.Store.Include(s => s.Orders).ToListAsync();
            var viewModel = new ListStoresViewModel()
            {
                Stores = stores,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ListUsers()
        {
            List<User> users = await UserManager.Users.ToListAsync();
            var currentUser = await GetCurrentUserAsync();
            IList<string> userRoles = await UserManager.GetRolesAsync(currentUser);

            var viewModel = new ListUsersViewModels
            {
                Users = users,
                CurrentUser = currentUser,
                CurrentUserRoles = userRoles
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            User user = await UserManager.FindByIdAsync(id);

            if (user == null)
            {

                return RedirectToAction("ListUsers");
            }

            // GetRolesAsync returns the list of user Roles
            IList<string> userRoles = await UserManager.GetRolesAsync(user);
            if (!userRoles.Any(r => r.Equals("Admin")))
            {

                return RedirectToAction("ListUsers");
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = userRoles,
                //ItemsInCart = await CountItemsInCart()
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            User user = await UserManager.FindByIdAsync(model.Id);

            if (user == null)
            {

                return RedirectToAction("ListUsers");
            }

            user.UserName = model.UserName;
            user.Email = model.Email;

            IdentityResult result = await UserManager.UpdateAsync(user);

            if (result.Succeeded)
            {

                return RedirectToAction("ListUsers");
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }


            return View(model);
        }

        //[HttpPost]
        //public async Task<IActionResult> DeleteUser(string id)
        //{
        //    User user = await UserManager.FindByIdAsync(id);

        //    if (user == null)
        //    {

        //        return RedirectToAction("ListUsers");
        //    }

        //    var currentUser = await GetCurrentUserAsync();

        //    IList<string> userRoles = await UserManager.GetRolesAsync(currentUser);

        //    if (!userRoles.Any(r => r.Equals("Admin")))
        //    {

        //        return RedirectToAction("ListUsers");
        //    }

        //    if (user.Id == currentUser.Id)
        //    {

        //        return RedirectToAction("ListUsers");
        //    }

        //    IdentityResult result = await UserManager.DeleteAsync(user);

        //    if (result.Succeeded)
        //    {

        //        return RedirectToAction("ListUsers");
        //    }

        //    foreach (IdentityError error in result.Errors)
        //    {
        //        ModelState.AddModelError(string.Empty, error.Description);
        //    }

        //    return View("ListUsers");
        //}

        [HttpGet]
        public async Task<IActionResult> ListRoles()
        {
            IQueryable<IdentityRole> roles = RoleManager.Roles;

            var viewModel = new ListRolesViewModel()
            {
                Roles = roles,
                //ItemsInCart = await CountItemsInCart()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            IdentityRole role = await RoleManager.FindByIdAsync(id);

            if (role == null)
            {

                return RedirectToAction("ListUsers");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name,
            };

            // Retrieve all the Users
            foreach (User user in UserManager.Users)
            {
                /*If the user is in this role, add the username to
                Users property of EditRoleViewModel. This model
                object is then passed to the view for display*/
                if (await UserManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            IdentityRole role = await RoleManager.FindByIdAsync(model.Id);

            if (role == null)
            {

                return RedirectToAction("ListUsers");
            }

            role.Name = model.RoleName;

            // Update the Role using UpdateAsync
            IdentityResult result = await RoleManager.UpdateAsync(role);

            if (result.Succeeded)
            {

                return RedirectToAction("ListRoles");
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }


            return View(model);
        }

        public async Task<IActionResult> EditUsersInRole(string roleName)
        {
            List<User> users = await UserManager.Users.ToListAsync();
            List<UserRoleViewModel> allusers = new List<UserRoleViewModel>();
            foreach (User user in users)
            {
                IList<string> userRoles = await UserManager.GetRolesAsync(user);
                if (userRoles.Contains(roleName))
                {
                    var viewModel = new UserRoleViewModel()
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        IsSelected = true
                    };
                    allusers.Add(viewModel);
                }
                else
                {
                    var viewModel = new UserRoleViewModel()
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        IsSelected = false
                    };
                    allusers.Add(viewModel);
                }
            }
            var viewModel2 = new ListUserRoleViewModel()
            {
                List = allusers,
                RoleName = roleName
            };
            return View(viewModel2);
        }


        //need to change
        public async Task<IActionResult> EditUsersInRole1(ListUserRoleViewModel model, string roleName)
        {
            for (var i = 0; i < model.List.Count; i++)
            {
                if (model.List[i].IsSelected)
                {
                    User user = await UserManager.FindByIdAsync(model.List[i].UserId);


                    var result = await UserManager.AddToRoleAsync(user, roleName);

                    if (result.Succeeded)
                    {

                        return RedirectToAction("ListRoles");
                    }

                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                }
                else
                {
                    User user = await UserManager.FindByIdAsync(model.List[i].UserId);
                    try
                    {
                        var result = await UserManager.RemoveFromRoleAsync(user, roleName);
                        if (result.Succeeded)
                        {

                            return RedirectToAction("ListRoles");
                        }

                        foreach (IdentityError error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            return View(model);
        }

        public async Task<IActionResult> ListOrders()
        {
            List<Order> orders = await _context.Order
                .Include(order => order.User)
                .Include(order => order.Payment)
                .Include(order => order.Store)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();
            var viewModel = new ListOrdersViewModel()
            {
                Orders = orders,
            };

            return View(viewModel);
        }



    }

}

