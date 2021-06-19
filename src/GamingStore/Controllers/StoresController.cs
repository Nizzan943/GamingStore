using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accuweather;
using GamingStore.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamingStore.Data;
using GamingStore.Models;
using GamingStore.Models.Relationships;
using GamingStore.ViewModels;
using GamingStore.ViewModels.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using RestSharp;

namespace GamingStore.Controllers
{
    public class StoresController : BaseController
    {
        private static readonly IAccuweatherApi _accuweatherApi = new AccuweatherApi("I7i66pnCspYN80KAWUGeAbMoo0dkdsLc");

        public StoresController(UserManager<User> userManager, GamingStoreContext context, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
            : base(userManager, context, roleManager, signInManager)
        {
        }

        // GET: Stores
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Store> stores = await Context.Store.ToListAsync();
            HashSet<string> uniqueCities = GetUniqueCities(stores);
            List<Store> openStores = GetOpenStores(stores);
            Dictionary<string, int> locations = new Dictionary<string, int>();
            foreach (var element in stores)
            {
                if (element.Name == "Website")
                    continue;

                if (locations.ContainsKey(element.Address.City))
                    continue;

                locations.Add(element.Address.City, element.LocationKey);
            }
            Dictionary<string, double> weather = GetCurrentWeather(uniqueCities);
            var viewModel = new StoresCitiesViewModel
            {
                Stores = stores,
                CitiesWithStores = uniqueCities.ToArray(),
                OpenStores = openStores,
                CurrentWeather = weather,
                ItemsInCart = await CountItemsInCart()
            };

            return View(viewModel);
        }

        private static List<Store> GetOpenStores(List<Store> stores)
        {
            List<Store> openStores = stores.Where(store => store.IsOpen()).ToList(); // get open stores

            return openStores;
        }

        // Post: Stores
        [HttpPost]
        public async Task<IActionResult> Index(StoresCitiesViewModel received)
        {
            List<Store> stores = await Context.Store.ToListAsync();
            HashSet<string> uniqueCities = GetUniqueCities(stores);

            // get open stores
            List<Store> openStores = stores.Where(store => store.IsOpen()).ToList();

            if (!string.IsNullOrEmpty(received.Name))
            {
                stores = stores.Where(store => store.Name.ToLower().Contains(received.Name.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(received.City))
            {
                stores = stores.Where(store => store.Address.City == received.City).ToList();
            }

            if (received.IsOpen)
            {
                stores = stores.Where(store => store.IsOpen()).ToList();
            }

            var viewModel = new StoresCitiesViewModel
            {
                Stores = stores,
                CitiesWithStores = uniqueCities.ToArray(),
                OpenStores = openStores,
                Name = received.Name,
                City = received.City,
                IsOpen = received.IsOpen,
                ItemsInCart = await CountItemsInCart()
            };

            return View(viewModel);
        }


        private static HashSet<string> GetUniqueCities(List<Store> stores)
        {
            // get stores with cities uniquely 
            var uniqueCities = new HashSet<string>();

            foreach (Store element in stores)
            {
                if (!string.IsNullOrWhiteSpace(element.Address.City))
                {
                    uniqueCities.Add(element.Address.City);
                }
            }

            return uniqueCities;
        }

        private static Dictionary<string, double> GetCurrentWeather(HashSet<string> stores)
        {
            var weather = new Dictionary<string, double>();

            foreach (var store in stores)
            {
                try
                {
                    if(store == String.Empty)
                        continue;
                    var client = new RestClient("http://api.weatherapi.com/v1/");
                    var request = new RestRequest("current.json", Method.GET);
                    request.AddParameter("key", "df7bc763685d44b68aa114413211906");
                    request.AddParameter("q", store);
                    var response = client.Execute(request);
                    var weatherByCity = JsonConvert.DeserializeObject<Root>(response.Content);
                    weather.Add(store, weatherByCity.current.temp_c);
                }
                catch (Exception e)
                {
                    continue;
                }
            }

            return weather;
        }

        // GET: Stores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Store store = await Context.Store.FirstOrDefaultAsync(m => m.Id == id);
            if (store == null)
            {
                return NotFound();
            }

            var viewModel = new StoreDetailsViewModel()
            {
                Store = store,
                ItemsInCart = await CountItemsInCart()
            };

            return View(viewModel);
        }

        // GET: Stores/Create
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateStoreViewModel()
            {
                Store = new Store()
                {
                    Active = true,
                    OpeningHours = new List<OpeningHours>(7)
                    {
                        new OpeningHours() {OpeningTime = new TimeSpan(00, 00, 00), ClosingTime = new TimeSpan(23, 59, 00), DayOfWeek = DayOfWeek.Sunday},
                        new OpeningHours() {OpeningTime = new TimeSpan(00, 00, 00), ClosingTime = new TimeSpan(23, 59, 00), DayOfWeek = DayOfWeek.Monday},
                        new OpeningHours() {OpeningTime = new TimeSpan(00, 00, 00), ClosingTime = new TimeSpan(23, 59, 00), DayOfWeek = DayOfWeek.Tuesday},
                        new OpeningHours() {OpeningTime = new TimeSpan(00, 00, 00), ClosingTime = new TimeSpan(23, 59, 00), DayOfWeek = DayOfWeek.Wednesday},
                        new OpeningHours() {OpeningTime = new TimeSpan(00, 00, 00), ClosingTime = new TimeSpan(23, 59, 00), DayOfWeek = DayOfWeek.Thursday},
                        new OpeningHours() {OpeningTime = new TimeSpan(00, 00, 00), ClosingTime = new TimeSpan(23, 59, 00), DayOfWeek = DayOfWeek.Friday},
                        new OpeningHours() {OpeningTime = new TimeSpan(00, 00, 00), ClosingTime = new TimeSpan(23, 59, 00), DayOfWeek = DayOfWeek.Saturday}
                    },
                    Address = new Address()
                    {
                        Country = "Israel"
                    }
                }
            };

            return View(viewModel);
        }

        // POST: Stores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,PhoneNumber,Email,OpeningHours")] Store store)
        {
            var viewModel = new CreateStoreViewModel()
            {
                Store = store,
            };

            if (store.OpeningHours.Any(openingHour => openingHour.ClosingTime <= openingHour.OpeningTime))
            {
                return RedirectToAction("ListStores", "Administration");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            Context.Add(store);
            await Context.SaveChangesAsync();

            return RedirectToAction("ListStores", "Administration");
        }

        // GET: Stores/Edit/5
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!StoreExists(id.Value))
            {
                return RedirectToAction("ListStores", "Administration");
            }

            Store store = await Context.Store.FindAsync(id);
            var viewModel = new StoreDetailsViewModel()
            {
                Store = store,
                ItemsInCart = await CountItemsInCart()
            };

            if (store == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        // POST: Stores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<IActionResult> Edit(Store store)
        {
            if (!StoreExists(store.Id))
            {
                return RedirectToAction("ListStores", "Administration");
            }

            // GetRolesAsync returns the list of user Roles
            IList<string> userRoles = await UserManager.GetRolesAsync(await GetCurrentUserAsync());

            if (!userRoles.Any(r => r.Equals("Admin")))
            {
                return RedirectToAction("ListStores", "Administration");
            }


            if (store.OpeningHours.Any(openingHour => openingHour.ClosingTime <= openingHour.OpeningTime))
            {

                return RedirectToAction("Edit", "Stores", new
                {
                    id = store.Id
                });
            }

            try
            {
                Context.Update(store);
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
            }

            return RedirectToAction("ListStores", "Administration");
        }

        // GET: Stores/Delete/5
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (!StoreExists(id.Value))
            {
                return RedirectToAction("ListStores", "Administration");
            }

            Store store = await Context.Store.FirstOrDefaultAsync(m => m.Id == id);
            var viewModel = new StoreDetailsViewModel()
            {
                Store = store,
                ItemsInCart = await CountItemsInCart()
            };
            if (store == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        // POST: Stores/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!StoreExists(id))
            {
                return RedirectToAction("ListStores", "Administration");
            }

            // GetRolesAsync returns the list of user Roles
            IList<string> userRoles = await UserManager.GetRolesAsync(await GetCurrentUserAsync());

            if (!userRoles.Any(r => r.Equals("Admin")))
            {
                return RedirectToAction("ListStores", "Administration");
            }


            Store store = await Context.Store.Include(s => s.Orders).FirstOrDefaultAsync(s => s.Id == id);

            if (store.Orders.Count > 0)
            {
                return RedirectToAction("ListStores", "Administration");
            }

            Context.Store.Remove(store);
            await Context.SaveChangesAsync();

            return RedirectToAction("ListStores", "Administration");
        }

        private bool StoreExists(int id)
        {
            return Context.Store.Any(e => e.Id == id);
        }
    }
}