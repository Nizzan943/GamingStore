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

namespace GamingStore.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly GamingStoreContext _context;

        public AdministrationController(GamingStoreContext context)
        {
            _context = context;
        }

        // GET: Administrator
        public ActionResult Index()
        {
            return View();
        }

        // GET: Administrator/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Administrator/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administrator/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Administrator/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Administrator/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Administrator/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Administrator/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
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
    }
}
