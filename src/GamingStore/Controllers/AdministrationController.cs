using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamingStore.Controllers
{
    public class AdministrationController : Controller
    {
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
    }
}
