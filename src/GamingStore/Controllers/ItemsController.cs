using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GamingStore.Data;
using GamingStore.Models;
using GamingStore.Services.Twitter;
using GamingStore.ViewModels.Items;
using Microsoft.AspNetCore.Authorization;

namespace GamingStore.Controllers
{
    public class ItemsController : Controller
    {
        private readonly GamingStoreContext _context;

        public ItemsController(GamingStoreContext context)
        {
            _context = context;
        }

        // Search
        public async Task<IActionResult> Search(string[] brands, string[] category, double price, string queryTitle)
        {
            var searchItems = _context.Item.Where(a => (brands.Contains(a.Brand) || brands.Length == 0) && (category.Contains(a.Category.Name)|| category.Length == 0) && (a.Price <= price || price == 0) && (a.Title.Contains(queryTitle) || queryTitle == null));
            ViewData["brands"] = brands.ToList();

            ViewData["category"] = category.ToList();

            ViewData["price"] = price;

            ViewData["queryTitle"] = queryTitle;

            return View("~/Views/Items/chosenIndex.cshtml", await searchItems.ToListAsync());
        }

        public async Task<IActionResult> CategoryItems(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var searchItems = _context.Item.Where(i => i.CategoryId == id);
            return View("~/Views/Items/Index.cshtml", await searchItems.ToListAsync());
        }

        // GET: Items
        public async Task<IActionResult> Index()
        {
            var gamingStoreContext = _context.Item.Include(i => i.Category);
            return View(await gamingStoreContext.ToListAsync());
        }

        // GET: Items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Items/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["Categories"] = new SelectList(_context.Category, nameof(Category.Id), nameof(Category.Name));
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEditItemViewModel model)
        {
            try
            {
                model.Item.Brand = model.Item.Brand.Trim();
                //string uploadFolder = await UploadImages(model);

                await _context.Item.AddAsync(model.Item);
                await _context.SaveChangesAsync();

                #region TwitterPost

                if (model.PublishItemFlag)
                {

                    //PublishTweet(model.Item, uploadFolder);
                }

                #endregion

            }
            catch (Exception e)
            {
            }

            return RedirectToAction("ListItems", "Administration");
        }

        // GET: Items/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", nameof(Category.Name), item.Category);
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Price,Brand,StockCounter,Description,CategoryId,StarReview,ImageUrl,Active")] Item item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Id", item.CategoryId);
            return View(item);
        }

        // GET: Items/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Item.FindAsync(id);
            _context.Item.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id)
        {
            return _context.Item.Any(e => e.Id == id);
        }

        private void PublishTweet(Item item, string itemImagesPath)
        {
            const string consumerKey = "rPwLqEA8pe0iEw8dvta4SxGE1";
            const string consumerKeySecret = "iySGb1uxhorSzFitQgxjYZRufiqPpa81gVQlSZvO088nqXrhtc";
            const string accessToken = "1406322532949630976-VzcCFalG79mlF8L5BEts0yRtwZoR4A";
            const string accessTokenSecret = "c8OeT0BZ5j4GBHrIdyBVApKSwNSVvJUI2FThW5NF6JiVw";

            var twitter = new Twitter(consumerKey, consumerKeySecret, accessToken, accessTokenSecret);

            string fullImageUrl = $"{itemImagesPath}\\1.jpg";
            string tweet = $"Gaming Store now sells {item.Title} only on {item.Price}$";

            try
            {

                string response = twitter.PublishToTwitter(tweet, fullImageUrl);
                Console.WriteLine(response);
            }
            catch (Exception e)
            {

            }
        }

    }
}

