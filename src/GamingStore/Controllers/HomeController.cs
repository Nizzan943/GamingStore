using GamingStore.Data;
using GamingStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GamingStore.Controllers
{
    public class HomeController : Controller
    {
        public static int flag = 0;

        private readonly ILogger<HomeController> _logger;

         private readonly GamingStoreContext _context;

        public HomeController(ILogger<HomeController> logger,GamingStoreContext context)
        {
            _logger = logger;

            _context = context;
        }

        public async Task<IActionResult> Search(string queryTitle)
        {
            var searchItems = _context.Item.Include(a => a.Category).Where(a => (a.Title.Contains(queryTitle) || queryTitle == null));
            return View("~/Views/Items/Index.cshtml", await searchItems.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (item == null)
            {
                return NotFound();
            }

            return View("~/Views/Items/Details.cshtml", item);
        }

        public async Task<IActionResult> IndexAsync()
        {
            return View(await _context.Category.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
