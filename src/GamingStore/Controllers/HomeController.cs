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
        ItemsController ItemsController;

        private readonly ILogger<HomeController> _logger;

         private readonly GamingStoreContext _context;

        public HomeController(ILogger<HomeController> logger,GamingStoreContext context)
        {
            _logger = logger;

            _context = context;

            ItemsController = new ItemsController(context);
        }

        public async Task<IActionResult> IndexAsync()
        {
            return View(await _context.Category.ToListAsync());
        }

        public IActionResult Contact()
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
