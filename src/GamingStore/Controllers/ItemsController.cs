using GamingStore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Data;
using GamingStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamingStore.ViewModels.Items;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using GamingStore.Services.Twitter;
using Microsoft.AspNetCore.Mvc.Rendering;
using GamingStore.Contracts;

namespace GamingStore.Controllers
{
    public class ItemsController : BaseController
    {
        private readonly GamingStoreContext _context;


        public ItemsController(UserManager<User> userManager, GamingStoreContext context, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
            : base(userManager, context, roleManager, signInManager)
        {
            _context = context;
        }

   
        // Search
        public async Task<IActionResult> Search(string[] brands, string[] category, double price, string queryTitle)
        {
            var searchItems = _context.Item.Where(a => (brands.Contains(a.Brand) || brands.Length == 0) && (category.Contains(a.Category.Name) || category.Length == 0) && (a.Price <= price || price == 0) && (a.Title.Contains(queryTitle) || queryTitle == null));
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
        public async Task<IActionResult> Index(int? pageNumber)
        {
            const int pageSize = 16;
            var gamingStoreContext = _context.Item.Include(i => i.Category);

            IQueryable<Item> items = _context.Item.Where(i => i.Active);

            List<string> brands = items.Select(i => i.Brand).Distinct().ToList();

            List<string> categories = items.Select(i => i.Category.Name).Distinct().ToList();

            PaginatedList<Item> paginatedList = await PaginatedList<Item>.CreateAsync(items.AsNoTracking(), pageNumber ?? 1, pageSize);

            var viewModel = new GetItemsViewModel()
            {
                Categories = categories,
                Brands = brands,
                Items = items.ToArray(),
                PaginatedItems = paginatedList,


                ItemsInCart = await CountItemsInCart()
            };

            

            return View(viewModel);
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

            var viewModel = new ItemViewModel()
            {
                Item = item,
                ItemsInCart = await CountItemsInCart()
            };

            return View(viewModel);
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
                string uploadFolder = await UploadImages(model);

                await _context.Item.AddAsync(model.Item);
                await _context.SaveChangesAsync();

                #region TwitterPost

                if (model.PublishItemFlag)
                {
                    PublishTweet(model.Item, uploadFolder);
                }

                #endregion TwitterPost
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

        private async Task<string> UploadImages(CreateEditItemViewModel model)
        {
            string directoryName = model.Item.Title.Trim().Replace(" ", string.Empty);
            string uploadFolder = Path.Combine("images", "items", directoryName);

            if (model.File1 != null)
            {
                await CopyImage(model, uploadFolder, 1);
                model.Item.ImageUrl = $"images/items/{directoryName}";
            }

            if (model.File2 != null)
            {
                await CopyImage(model, uploadFolder, 1);
            }

            if (model.File3 != null)
            {
                await CopyImage(model, uploadFolder, 1);
            }

            return uploadFolder;
        }

        private static async Task CopyImage(CreateEditItemViewModel model, string uploadFolder, int imageNumber)
        {
            Directory.CreateDirectory(uploadFolder);
            string uniqueFileName = $"{imageNumber}.jpg";
            string filePath = Path.Combine(uploadFolder, uniqueFileName);
            var fileStream = new FileStream(filePath, FileMode.Create);
            await model.File1.CopyToAsync(fileStream);
            fileStream.Close();
        }

        [Authorize]
        public async Task<IActionResult> AddToCart(int itemId, int quantity = 1)
        {
            try
            {
                if (quantity == 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                User user = await GetCurrentUserAsync();

                if (user == null) // Not Log In
                {
                    return NotFound();
                }

                Cart cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == user.Id && c.ItemId == itemId);

                if (cart == null)
                {
                    cart = new Cart()
                    {
                        UserId = user.Id,
                        ItemId = itemId,
                        Quantity = quantity
                    };

                    await _context.Cart.AddAsync(cart);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                cart.Quantity = cart.Quantity + quantity;
                _context.Update(cart);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                
            }

            return RedirectToAction(nameof(Index));
        }





    }
}