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
using Microsoft.AspNetCore.Http;

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
        public async Task<IActionResult> Search(string[] brands, string[] category, double price, string queryTitle, int? pageNumber)
        {
            var searchItems = _context.Item.Where(a => (brands.Contains(a.Brand) || brands.Length == 0) && (category.Contains(a.Category.Name) || category.Length == 0) && (a.Price <= price || price == 0) && (a.Title.Contains(queryTitle) || queryTitle == null) && (a.Active));

            const int pageSize = 200;

            List<string> theBrands = brands.ToList();

            List<string> theCategories = category.ToList();

            PaginatedList<Item> paginatedList = await PaginatedList<Item>.CreateAsync(searchItems.AsNoTracking(), pageNumber ?? 1, pageSize);

            double thePrice = price;

            string theQueryTitle = queryTitle;

            //for all the items

            IQueryable<Item> allItems = _context.Item.Where(i => i.Active);

            List<string> allBrands = allItems.Select(i => i.Brand).Distinct().ToList();

            List<string> allCategories = allItems.Select(i => i.Category.Name).Distinct().ToList();


            var viewModel = new GetChosenItemsViewModel()
            {
                Categories = theCategories,
                Brands = theBrands,
                Items = searchItems.ToArray(),
                PaginatedItems = paginatedList,
                Price = thePrice,
                QueryTitle = theQueryTitle,

                AllItems = allItems.ToArray(),
                AllBrands = allBrands,
                AllCategories = allCategories,

                ItemsInCart = await CountItemsInCart()
            };

            return View("~/Views/Items/chosenIndex.cshtml", viewModel);
        }


        public async Task<IActionResult> CategoryItems(int? id, int? pageNumber)
        {
            var searchItems = _context.Item.Where(i => i.CategoryId == id && i.Active);

            const int pageSize = 200;

            List<string> theBrands = new List<string>();

            List<string> theCategories = new List<string>();

            foreach (var category in _context.Category)
            {
                if (category.Id == id)
                {
                    theCategories.Add(category.Name);
                    break;
                }
            }

            PaginatedList<Item> paginatedList = await PaginatedList<Item>.CreateAsync(searchItems.AsNoTracking(), pageNumber ?? 1, pageSize);

            //for all the items

            IQueryable<Item> allItems = _context.Item.Where(i => i.Active);

            List<string> allBrands = allItems.Select(i => i.Brand).Distinct().ToList();

            List<string> allCategories = allItems.Select(i => i.Category.Name).Distinct().ToList();

            var viewModel = new GetChosenItemsViewModel()
            {
                Categories = theCategories,
                Brands = theBrands,
                Items = searchItems.ToArray(),
                PaginatedItems = paginatedList,
                Price = 0.0,
                QueryTitle = null,

                AllItems = allItems.ToArray(),
                AllBrands = allBrands,
                AllCategories = allCategories,

                ItemsInCart = await CountItemsInCart()
            };

            if (id == null)
            {
                return NotFound();
            }

            return View("~/Views/Items/chosenIndex.cshtml", viewModel);
        }

        // GET: Items
        public async Task<IActionResult> Index(int? pageNumber)
        {
            const int pageSize = 200;

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
        public async Task<IActionResult> Create(CreateItemViewModel model)
        {
            try
            {
                model.Item.Brand = model.Item.Brand.Trim();
                string uploadFolder = await UploadImages(model);
                string dir = "wwwroot\\" + uploadFolder;
                System.IO.Directory.CreateDirectory(dir);
                string fileName1 = "1.jpg";
                string fileName2 = "2.jpg";
                string fileName3 = "3.jpg";

                string file1 = System.IO.Path.Combine(dir, fileName1);
                string file2 = System.IO.Path.Combine(dir, fileName2);
                string file3 = System.IO.Path.Combine(dir, fileName3);

                using (Stream fileStream = new FileStream(file1, FileMode.Create))
                {
                    await model.File1.CopyToAsync(fileStream);
                }

                using (Stream fileStream = new FileStream(file2, FileMode.Create))
                {
                    await model.File2.CopyToAsync(fileStream);
                }

                using (Stream fileStream = new FileStream(file3, FileMode.Create))
                {
                    await model.File3.CopyToAsync(fileStream);
                }

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

            string[] files = Directory.GetFiles("wwwroot\\images\\items\\" + item.Title.Replace(" ", ""));

            IFormFile file1;
            IFormFile file2;
            IFormFile file3;

            using (var stream = System.IO.File.OpenRead(files[0]))
            {
                file1 = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));
            }
            using (var stream = System.IO.File.OpenRead(files[1]))
            {
                file2 = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));
            }
            using (var stream = System.IO.File.OpenRead(files[2]))
            {
                file3 = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));
            }

            IQueryable<Item> allItems = _context.Item.Where(i => i.Active);

            List<Category> allCategories = await _context.Category.ToListAsync();

            var viewModel = new EditItemViewModel()
            {
                Item = item,
                LastItemName = item.Title,
                File1 = file1,
                File2 = file2,
                File3 = file3,

                categories = allCategories
            };

            //ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", nameof(Category.Name), item.Category);
            return View(viewModel);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditItemViewModel model)
        {
                try
                {
                    if (model.LastItemName != model.Item.Title)
                    {
                        if (!Directory.Exists("wwwroot\\images\\items\\" + model.Item.Title))
                            Directory.Move("wwwroot\\images\\items\\" + model.LastItemName, "wwwroot\\images\\items\\" + model.Item.Title);
                    string temp = Path.Combine("images", "items", model.Item.Title);
                    model.Item.ImageUrl = temp;
                    }
                    if (model.File1 != null)
                    {
                        string dir = "wwwroot\\images\\items\\" + model.Item.Title;
                        string fileName1 = "1.jpg";

                        string _file1 = System.IO.Path.Combine(dir, fileName1);

                        using (Stream fileStream = new FileStream(_file1, FileMode.Create))
                        {
                            await model.File1.CopyToAsync(fileStream);
                        }
                    }

                    if (model.File2 != null)
                    {
                        string dir = "wwwroot\\images\\items" + model.Item.Title;
                        string fileName2 = "2.jpg";

                        string _file2 = System.IO.Path.Combine(dir, fileName2);

                        using (Stream fileStream = new FileStream(_file2, FileMode.Create))
                        {
                            await model.File2.CopyToAsync(fileStream);
                        }
                    }

                    if (model.File3 != null)
                    {
                        string dir = "wwwroot\\images\\items" + model.Item.Title;
                        string fileName3 = "3.jpg";

                        string _file3 = System.IO.Path.Combine(dir, fileName3);

                        using (Stream fileStream = new FileStream(_file3, FileMode.Create))
                        {
                            await model.File3.CopyToAsync(fileStream);
                        }
                    }
                    _context.Update(model.Item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(model.Item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            return RedirectToAction("ListItems", "Administration");
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
            return RedirectToAction("ListItems", "Administration");
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

        private async Task<string> UploadImages(CreateItemViewModel model)
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
                await CopyImage(model, uploadFolder, 2);
            }

            if (model.File3 != null)
            {
                await CopyImage(model, uploadFolder, 3);
            }

            return uploadFolder;
        }

        private static async Task CopyImage(CreateItemViewModel model, string uploadFolder, int imageNumber)
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