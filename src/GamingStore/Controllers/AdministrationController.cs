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
        public ActionResult Index()
        {
            return View();
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

    }

}

