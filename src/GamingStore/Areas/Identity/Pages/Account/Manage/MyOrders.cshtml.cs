using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Contracts;
using GamingStore.Data;
using GamingStore.Models;
using GamingStore.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GamingStore.Areas.Identity.Pages.Account.Manage
{
    public class MyOrdersModel : ViewPageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly GamingStoreContext _context;

        public MyOrdersModel(UserManager<User> userManager, ILogger<PersonalDataModel> logger, GamingStoreContext context) 
            : base(context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<Order> Orders { get; set; }
        public string StatusMessage { get; set; }

        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public List<Order> Orders { get; set; }
        }

        private void Load(List<Order> orders)
        {
            Orders = orders;

            Input = new InputModel
            {
                Orders = Orders
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            User user = await _userManager.GetUserAsync(User);
            ItemsInCart = await CountItemsInCart(user);
            
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var query = _context.Order.
                Join(_context.User, order => order.UserId, customer => customer.Id, (order, customer) => new { order, customer }).
                Join(_context.Payment, combinedOrder => combinedOrder.order.PaymentId, payment => payment.Id, (combinedOrder, payment) => new
                {
                    Payment = payment,
                    Order = combinedOrder.order,
                    Customer = combinedOrder.customer
                }).
                Where(c => c.Customer.Id == user.Id).
                Take(int.MaxValue);

            var ordersList = new List<Order>();

            foreach (var queryResult in query)
            {
                ordersList.Add(queryResult.Order);
            }

            ordersList.Sort((x, y) => y.OrderDate.CompareTo(x.OrderDate));
            Load(ordersList);

            return Page();
        }
    }
}