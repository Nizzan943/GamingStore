using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Contracts;
//using GamingStore.Extensions;
using GamingStore.Models;
using GamingStore.Models.Relationships;
//using GamingStore.Models.Relationships;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using static GamingStore.Contracts.PaymentMethod;

namespace GamingStore.Data
{
    public class SeedData
    {
        private static readonly string DirectoryPath =
            AppContext.BaseDirectory.Substring(0,
                AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal));
        private static UserManager<User> _userManager;

        private static RoleManager<IdentityRole> _roleManager;

        public static async Task Initialize(IServiceProvider serviceProvider, string adminPassword)
        {
            await using var context = new GamingStoreContext(serviceProvider.GetRequiredService<DbContextOptions<GamingStoreContext>>());
            _userManager = serviceProvider.GetService<UserManager<User>>();
            _roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            await CreateRolesAndUsers(context, adminPassword);

            SeedDatabase(context);
        }

        private static async Task CreateRolesAndUsers(GamingStoreContext context, string adminPassword)
        {
            const string admin = "Admin";
            const string User = "User";


            bool roleExists = await _roleManager.RoleExistsAsync(admin);

            if (!roleExists)
            {
                // first we create Admin roll    
                var role = new IdentityRole { Name = admin };
                await _roleManager.CreateAsync(role);

                //Here we create a Admin super users who will maintain the website                   
                await AddAdmins(_userManager, adminPassword);
            }
            await AddAdmins(_userManager, adminPassword);

            // creating Creating Employee role     
            roleExists = await _roleManager.RoleExistsAsync(User);
            if (!roleExists)
            {
                var role = new IdentityRole { Name = User };
                await _roleManager.CreateAsync(role);

                //Here we create a mock Users super users who will maintain the website                   
                await AddUsers(_userManager, "default123");
            }
            await context.SaveChangesAsync();
        }

        private static async Task AddUsers(UserManager<User> userManager, string usersPassword)
        {
            string dataUsers = await System.IO.File.ReadAllTextAsync($@"{DirectoryPath}\Data\Mock_Data\UsersMin.json");
            List<User> Users = JsonConvert.DeserializeObject<List<User>>(dataUsers);


            foreach (User User in Users)
            {
                IdentityResult result = await userManager.CreateAsync(User, usersPassword);

                //Add default User to Role Admin    
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(User, "User");
                }
            }
        }

        private static async Task AddAdmins(UserManager<User> userManager, string adminPassword)
        {
            var admins = new List<User>
            {
                new User
                {
                    UserName = "nizzan943@gmail.com",
                    Email = "nizzan943@gmail.com",
                    FirstName = "Nitzan",
                    LastName = "Miranda"
                },
                new User
                {
                    UserName = "yuval.amir998@gmail.com",
                    Email = "yuval.amir998@gmail.com",
                    FirstName = "Yuval",
                    LastName = "Amir"
                },
                new User
                {
                    UserName = "oribachar98@gmail.com",
                    Email = "oribachar98@gmail.com",
                    FirstName = "Ori",
                    LastName = "Bachar"
                }
            };

            foreach (User newAdmin in admins)
            {
                IdentityResult result = await userManager.CreateAsync(newAdmin, adminPassword);

                //Add default User to Role Admin    
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }

        public static void SeedDatabase(GamingStoreContext context)
        {
            
            if (context.Item.Any())
            {
                return;
            }
            
            string directoryPath =
                AppContext.BaseDirectory.Substring(0,
                    AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal));

            
            if (context.Category.Any())
            {
                return;
            }
            
            var categories = SeedCategories(context);

            var items = SeedItems(context);

            var stores = SeedStores(context, directoryPath);

            //SeedOrdersAndPayments(context, items, stores);
        }


        private static List<Store> SeedStores(GamingStoreContext context, string directoryPath)
        {
            string dataStores = System.IO.File.ReadAllText(directoryPath + @"\Data\Store.json");
            List<Store> stores = JsonConvert.DeserializeObject<List<Store>>(dataStores);

            if (context.Item.Any() && context.User.Any())
            {
                //GenerateStoreItems(stores, items, context.Customers.ToList());
            }

            context.Store.AddRange(stores);
            context.SaveChanges();

            return stores;
        }

        private static void GenerateStoreItems(IEnumerable<Store> stores, Item[] items, List<User> customersList)
        {
            var random = new Random();

            foreach (Store store in stores)
            {
                foreach (Item item in items)
                {
                    bool itemCreated = random.Next(2) == 1; // 1 - True  - False

                    if (!itemCreated)
                    {
                        continue;
                    }

                    const float itemsNumberMultiplier = 0.3f;

                    store.StoreItems.Add(new StoreItem
                    {
                        ItemId = item.Id,
                        StoreId = store.Id,
                        ItemsCount =
                            (uint)random.Next(1,
                                (int)(customersList.Count * itemsNumberMultiplier)) // customers number times 0.3
                    });
                }
            }
        }

        //private static void SeedOrdersAndPayments(GamingStoreContext context, Item[] items, List<Store> stores)
        //{
        //    IEnumerable<Order> orders = GenerateOrders(context.Users.AsEnumerable(), items.ToList(), stores, out List<Payment> payments);

        //    List<Order> orderList = orders.ToList();
        //    foreach (Order order in orderList)
        //    {
        //        context.Orders.Add(order);
        //    }

        //    foreach (Payment payment in payments)
        //    {
        //        context.Payments.Add(payment);
        //    }

        //    context.Orders.AsNoTracking();
        //    context.SaveChanges();
        //}



        private static Category[] SeedCategories(GamingStoreContext context)
        {
            var categories = new[]
            {
                new Category
                {
                    Name = "GPUs",
                    Image = "images/categories/gpus.jpg",
                },
                new Category
                {
                    Name = "CPUs",
                    Image = "images/categories/cpus.jpg",
                },
                new Category
                {
                    Name = "Keyboards",
                    Image = "images/categories/keyboards.jpg",
                },
                new Category
                {
                    Name = "Mouses",
                    Image = "images/categories/mouses.jpg",
                },
                new Category
                {
                    Name = "Monitors",
                    Image = "images/categories/monitors.jpg",
                },
                new Category
                {
                    Name = "Chairs",
                    Image = "images/categories/gaming chairs.jpg",
                },
                new Category
                {
                    Name = "Headsets",
                    Image = "images/categories/gaming headsets.jpg",
                },
                new Category
                {
                    Name = "Pads",
                    Image = "images/categories/mouse pads.jpg",
                }
            };

            foreach (var category in categories)
            {
                context.Category.Add(category);
            }

            context.Category.AsNoTracking();
            context.SaveChanges();
            return categories;
        }

        private static Item[] SeedItems(GamingStoreContext context)
        {

            // Load items if they don't exist.
            var items = new[]
            {
                new Item
                {
                    Title = "Cloud Stinger Wired Stereo Gaming Headset",
                    Brand = "HyperX",
                    Price = 29.78,
                    Category = context.Category.Find(7),
                    ImageUrl = "images/items/CloudStingerWiredStereoGamingHeadset",
                    Description = "HyperX Cloud Stinger is the ideal headset for gamers looking for lightweight comfort, superior sound quality and added convenience. At just 275 grams, it’s comfortable on your neck and its ear cups rotate in a 90-degree angle for a better fit. HyperX signature memory foam also provides ultimate comfort around the ears for prolonged gaming sessions."
                },
                new Item
                {
                    Title = "G432 Wired 7.1 Surround Sound Gaming Headset", Brand = "Logitech", Price = 39.95,
                    Category = context.Category.Find(7), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Built-In Microphone", "Yes"},
                        {"Connection Type", "Wired"},
                        {"Headphone Fit", "Over-the-Ear"}
                    },
                    ImageUrl = "images/items/G432Wired7dot1SurroundSoundGamingHeadset",
                    Description = "Logitech G432 7. 1 surround sound gaming headset is enhanced with advanced Soundscape technology. Hear more of the game with huge 50 mm drivers that deliver a big sound. For maximum immersion, DTS Headphone: X 2. 0 surround sound creates precise in-game positional awareness."
                },
                new Item
                {
                    Title = "Milano Gaming Chair Green", Brand = "Arozzi", Price = 227,
                    Category = context.Category.Find(6), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Office Chair Style", "Gaming chair"},
                        {"Headrest Features", "Padded"},
                        {"Swivel Angle", "360 degrees"},
                        {"Color", "Green"}
                    },
                    ImageUrl = "images/items/MilanoGamingChair-Green",
                    Description = "No matter where you game, work or even just read and relax, doing it in supreme comfort allows you to do it better, longer and with greater enthusiasm. That’s the inspiration for Milano, Arozzi’s gaming chair which provides both maximum comfort and maximum value."
                },
                new Item
                {
                    Title = "Milano Gaming Chair Red", Brand = "Arozzi", Price = 219.99,
                    Category = context.Category.Find(6), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Office Chair Style", "Gaming chair"},
                        {"Headrest Features", "Padded"},
                        {"Swivel Angle", "360 degrees"},
                        {"Color", "Blue"}
                    },
                    ImageUrl = "images/items/MilanoGamingChair-Red",
                    Description = "No matter where you game, work or even just read and relax, doing it in supreme comfort allows you to do it better, longer and with greater enthusiasm. That’s the inspiration for Milano, Arozzi’s gaming chair which provides both maximum comfort and maximum value."
                },
                new Item
                {
                    Title = "Logitech G440 Hard Gaming", Brand = "Logitech", Price = 130,
                    Category = context.Category.Find(8), PropertiesList = new Dictionary<string, string>()
                    {
                        {"length", "280mm"},
                        {"width", "340mm"},
                        {"height", "3mm"}
                    },
                    ImageUrl = "images/items/LogitechG440HardGaming",
                    Description = "G440 features a low friction, hard polymer surface ideal for high DPI gaming, improving mouse control and precise cursor placement."
                },
                new Item
                {
                    Title = "NVIDIA GeForce RTX 3080", Brand = "NVIDIA", Price = 719,
                    Category = context.Category.Find(1),
                    PropertiesList = new Dictionary<string, string>()
                    {
                        {"Cooling System", "Fan"},
                        {"Boost Clock Speed", "1.71 GHz"},
                        {"GPU Memory Size", "10 GB"}
                    },
                    ImageUrl = "images/items/NVIDIAGeForceRTX3080",
                    Description = "NVIDIA Ampere Streaming Multiprocessors 2nd Generation RT Cores 3rd Generation Tensor Cores Powered by GeForce RTX 3080 Integrated with 10GB GDDR6X 320-bit memory interface WINDFORCE 3X Cooling System with alternate spinning fans RGB Fusion 2.0 Protection metal back plate Clock Core: 1755"
                },
                new Item
                {
                    Title = "NVIDIA GeForce RTX 3090", Brand = "NVIDIA", Price = 1500, Category = context.Category.Find(1),
                    PropertiesList = new Dictionary<string, string>()
                    {
                        {"Cooling System", "Fan"},
                        {"Boost Clock Speed", "1.70 GHz"},
                        {"GPU Memory Size", "24 GB"}
                    },
                    ImageUrl = "images/items/NVIDIAGeForceRTX3090",
                    Description = "The GeForce RTXTM 3090 is a big ferocious GPU (BFGPU) with TITAN class performance. It’s powered by Ampere—NVIDIA’s 2nd gen RTX architecture—doubling down on ray tracing and AI performance with enhanced RT Cores, Tensor Cores, and new streaming multiprocessors. Plus, it features a staggering 24 GB of G6X memory"
                },
                new Item
                {
                    Title = "GeForce RTX 2080 SUPER BLACK GAMING", Brand = "EVGA", Price = 430,
                    Category = context.Category.Find(1),
                    PropertiesList = new Dictionary<string, string>()
                    {
                        {"Cooling System", "Active"},
                        {"Boost Clock Speed", "1815 MHz"},
                        {"GPU Memory Size", "8 GB"}
                    },
                    ImageUrl = "images/items/GeForceRTX2080SUPERBLACKGAMING",
                    Description = "The EVGA GeForce RTX K-series graphics cards are powered by the all-New NVIDIA Turing architecture to give you incredible New levels of gaming realism, speed, power efficiency, and immersion. With the EVGA GeForce RTX K-series gaming cards you get the best gaming experience with the next generation graphics performance"
                },
                new Item
                {
                    Title = "Intel Core i9-10900KA Comet Lake Box", Brand = "Intel", Price = 2420,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Cores", "10"},
                        {"Clock Speed", "3.60GHz - 5.30GHz"},
                        {"GPU", "Intel® UHD Graphics 630"},
                        {"Cache Memory", "20MB"}
                    },
                    ImageUrl = "images/items/IntelCorei9-10850KACometLakeBox",
                    Description = "Intel BX80684I99900KF Intel Core i9-9900KF Desktop Processor 8 Cores up to 5. 0 GHz Turbo Unlocked Without Processor Graphics LGA1151 300 Series 95W. Memory Types: DDR4-2666,Max Memory Bandwidth: 41.6 GB/s, Scalability: 1S Only,PCI Express Configurations: Up to 1x16, 2x8, 1x8+2x4"
                },
                new Item
                {
                    Title = "Intel Core i9-10850KA Comet Lake Box", Brand = "Intel", Price = 2020,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Cores", "10"},
                        {"Clock Speed", "3.60GHz - 5.20GHz"},
                        {"GPU", "Intel® UHD Graphics 630"},
                        {"Cache Memory", "20MB"}
                    },
                    ImageUrl = "images/items/IntelCorei9-10900KACometLakeBox",
                    Description = "Intel BX80684I99900KF Intel Core i9-9900KF Desktop Processor 8 Cores up to 5. 0 GHz Turbo Unlocked Without Processor Graphics LGA1151 300 Series 95W. Memory Types: DDR4-2666,Max Memory Bandwidth: 41.6 GB/s, Scalability: 1S Only,PCI Express Configurations: Up to 1x16, 2x8, 1x8+2x4"
                },
                new Item
                {
                    Title = "Gaming Headset white combat camouflage", Brand = "Dragon", Price = 499,
                    Category = context.Category.Find(7), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Sound Mode", "Stereo"},
                        {"Connection Type", "Wired"},
                        {"Water Resistant", "No"}
                    },
                    ImageUrl = "images/items/gamingheadsetwhitecombatcamouflage",
                    Description = "High precision 50mm magnetic neodymium drivers deliver high-quality stereo sound, From clear high-frequency and mid-range playback to deep bass surround bass. provide you a immerse game experience, Let you quickly hear footsteps and distant gunshots from different direction in Fortnight, PUBG or CS: go etc"
                },
                new Item
                {
                    Title = "Sceptre 24' Curved 75Hz Gaming LED Monitor", Brand = "Sceptre", Price = 129,
                    Category = context.Category.Find(5), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Display Size", "24 Inches"},
                        {"Resolution", "FHD 1080p"},
                        {"Hardware Interface", "VGA, HDMI"},
                        {"Display Technology", "LED"}
                    },
                    ImageUrl = "images/items/Sceptre24Curved",
                    Description = "24' Diagonal viewable curved screen HDMI, VGA & PC audio in ports Windows 10 compatible Contemporary sleek metal design with the C248W-1920RN, a slender 1800R screen curvature yields wide-ranging images that seemingly surround you. Protection and comfort are the hallmarks of this design as the metal pattern brush fi₪h is smooth and pleasing to the touch."
                },
                new Item
                {
                    Title = "AMD Ryzen 9 3900X 12-core, 24-thread processor", Brand = "AMD", Price = 429,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Processor Speed", "4.6 GHz"},
                        {"Processor Socket", "Socket AM4"}
                    },
                    ImageUrl = "images/items/AMDRyzen3900X",
                    Description = "AMD Ryzen 9 3900X 12 core, 24 thread unlocked desktop processor with Wraith Prism LED cooler."
                },
                new Item
                {
                    Title = "Acer Predator XB271HU 27' WQHD Monitor", Brand = "Acer", Price = 510.9,
                    Category = context.Category.Find(5), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Size", "27'"},
                        {"Technology", "NVIDIA G-SYNC"},
                        {"Resolution", "2560 x 1440 WQHD"},
                        {"Refresh Rate", "144Hz (OverClocking to 165Hz)"},
                        {"Panel Type", "IPS"},
                        {"Response Time", "4ms"},
                        {"Ports", "1 x DP, 1 x HDMI & 4 x USB 3.0"}
                    },
                    ImageUrl = "images/items/AcerPredatorXB271HU",
                    Description = "Fasten your seatbelt: Acer's Predator XB271HU WQHD display is about to turbocharge your gaming experience. This monitor combines jaw dropping specs, IPS panel that supports 144Hz refresh rate, delivering an amazing gaming experience."
                },
                new Item
                {
                    Title = "Asus VG278QR 27” Gaming Monitor", Brand = "Asus", Price = 336.9,
                    Category = context.Category.Find(5), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Size", "27'"},
                        {"Technology", "NVIDIA G-SYNC"},
                        {"Resolution", "FHD 1080p Ultra Wide"},
                        {"Refresh Rate", "165Hz"},
                        {"Response Time", "0.5ms"},
                        {"Ports", "DisplayPort, HDMI"}
                    },
                    ImageUrl = "images/items/AsusVG278QR27GamingMonitor",
                    Description = "Make every millisecond count with the 27” VG278QR gaming monitor featuring a 165Hz refresh rate and 0 5ms response time with Asus’ elmb technology to reduce motion blur with free Sync and G-SYNC compatibility Turn any desk into a marathon battle station with vg278qr’s ergonomic adjustable stand and eye Care technology."
                },
                new Item
                {
                    Title = "LG 27GL83A-B 27 Inch Ultragear QHD IPS 1ms", Brand = "LG", Price = 379.99,
                    Category = context.Category.Find(5), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Size", "27'"},
                        {"Technology", "NVIDIA G-SYNC"},
                        {"Resolution", "QHD Wide 1440p"},
                        {"Response Time", "1ms"}
                    },
                    ImageUrl = "images/items/LG27GL83A-B",
                    Description = "The 27” Ultra Gear QHD IPS 1ms 144Hz monitor is G-Sync Compatible and has a 3-Side Virtually Borderless bezel. Other features includes: Tilt / Height / Pivot Adjustable Stand."
                },
                new Item
                {
                    Title = "Logitech G PRO Mechanical Gaming Keyboard", Brand = "Logitech", Price = 118.99,
                    Category = context.Category.Find(3), PropertiesList = new Dictionary<string, string>()
                    {
                        {"LIGHTSPEED Wireless", "No"},
                        {"Mechanical Switches", "GX Blue clicky"},
                        {"Connectivity", "USB Keyboard + USB Passthrough"},
                        {"Dedicated Media Control", "No - Integrated F-keys"}
                    },
                    ImageUrl = "images/items/LogitechGPROMechanicalGamingKeyboard",
                    Description = "Built for pros from the bottom up. A compact tenkeyless design frees up table space for low-sens mousing. Pro-grade Clicky switches give you an audible feedback bump. Programmable LIGHTSYNC RGB and onboard memory lets you customize and store a lighting pattern for tournaments"
                },
                new Item
                {
                    Title = "Corsair Strafe RGB MK.2 Mechanical Gaming Keyboard", Brand = "Corsair", Price = 127.71,
                    Category = context.Category.Find(3), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Connections", "USB"}
                    },
                    ImageUrl = "images/items/CorsairStrafe",
                    Description = "The next-generation CORSAIR STRAFE RGB MK.2 mechanical keyboard features 100% CHERRY MX Silent RGB keyswitches for key presses that are up to 30% quieter, alongside and 8MB onboard profile storage to take your gaming profiles with you."
                },
                new Item
                {
                    Title = "Mouse M325 Lemon", Brand = "Logitech", Price = 29.95,
                    Category = context.Category.Find(4), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Mouse Shape", "Right-Handed"},
                        {"Connection Type", "Wireless"},
                        {"Weight", "125g"},
                        {"Max battery life", "36 Months"}
                    },
                    ImageUrl = "images/items/LogitechM325MouseLemon",
                    Description = "Logitech Wireless Mouse M325 Lemon Yellow With micro-precise scrolling, ultra-smooth cursor control and super-long-and-reliable battery power, the compact Logitech Wireles Mouse M325 screams control—and personal style in your choice of sweet color combinations."
                },
                new Item
                {
                    Title = "M325 Mouse Watermelon", Brand = "Logitech", Price = 20.58,
                    Category = context.Category.Find(4), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Mouse Shape", "Right-Handed"},
                        {"Connection Type", "Wireless"},
                        {"Weight", "140g"},
                        {"Max battery life", "36 Months"}
                    },
                    ImageUrl = "images/items/LogitechM325MouseWatermelon",
                    Description = "Logitech M325 Wireless Mouse With micro-precise scrolling, ultra-smooth cursor control and super-long-and-reliable battery power, the compact Logitech Wireles Mouse M325 screams control—and personal style in your choice of sweet color combinations."
                },
                new Item
                {
                    Title = "M525 Mouse", Brand = "Logitech", Price = 20.58,
                    Category = context.Category.Find(4), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Mouse Shape", "Right-Handed"},
                        {"Connection Type", "Wireless"},
                        {"Weight", "156g"},
                        {"Max battery life", "36 Months"}
                    },
                    ImageUrl = "images/items/LogitechM525",
                    Description = "With micro-precise scrolling, ultra-smooth cursor control and super-long-and-reliable battery power, the compact Logitech Wireles Mouse M525 screams control—and personal style in your choice of sweet color combinations."
                },
                new Item
                {
                    Title = "Basilisk Essentiy", Brand = "Razer", Price = 45.55,
                    Category = context.Category.Find(4), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Mouse Shape", "Right-Handed"},
                        {"Connection Type", "Wired"},
                        {"Weight", "127g"},
                        {"Programmable Buttons", "7"}
                    },
                    ImageUrl = "images/items/RazerBasiliskEssentiy",
                    Description = "The Razer Basilisk Essential is the gaming mouse with customizable features for an edge in battle. The multi-function paddle offers extended controls such as push-to-talk, while the Razer mechanical mouse switches give fast response times and are durable for up to 20 million clicks."
                },
                new Item
                {
                    Title = "DeathAdder v2 Ergonomic", Brand = "Razer", Price = 69.99,
                    Category = context.Category.Find(4), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Mouse Shape", "Right-Handed"},
                        {"Connection Type", "Wired"},
                        {"Weight", "82g"},
                        {"Programmable Buttons", "8"}
                    },
                    ImageUrl = "images/items/RazerDeathAdderv2Ergonomic",
                    Description = "With over 10 million Razer DeathAdders sold, the most celebrated and awarded gaming mouse in the world has earned its popularity through its exceptional ergonomic design. Perfectly suited for a palm grip, it also works well with claw and fingertip styles. The Razer DeathAdder V2 continues this legacy, retaining its signature shape while shedding more weight for quicker handling to improve your gameplay. Going beyond conventional office ergonomics, the optimized design also provides greater comfort for gaming—important for those long raids or when you’re grinding your rank on ladder."
                },
                //START HERE.................
                new Item
                {
                    Title = "AMD Ryzen 5 3600XT 6-core, 12-threads unlocked", Brand = "AMD", Price = 233.30,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Processor Speed", "4.5 GHz"},
                        {"Processor Socket", "Socket AM4"},
                        {"TDP", "95W"},
                        {"GameCache", "35MB"}
                    },
                    ImageUrl = "images/items/AMDRyzen53600XT",
                    Description = "The AMD Ryzen 5 3600XT. Light Years Ahead. Featuring award winning performance and optimized technology for gamers, for creators, for everyone."
                },
                new Item
                {
                    Title = "Intel Core i5-8400 Desktop Processor 6 Cores", Brand = "Intel", Price = 205,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Processor Speed", "2.8 GHz"},
                        {"Processor Socket", "LGA 1151"},
                        {"UHD Graphics", "630"},
                        {"Smart Cache", "9MB"}
                    },
                    ImageUrl = "images/items/IntelCorei5-8400",
                    Description = "Intel Core i5-8400 Processor (9M Cache, up to 2.80 GHz)"
                },

                new Item
                {
                    Title = "Intel Core i3-9100F Desktop Processor 4 Core", Brand = "Intel", Price = 91.95,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Processor Speed", "3.6 GHz"},
                        {"Processor Socket", "LGA 1151"},
                        {"TDP", "65W"},
                        {"Smart Cache", "6MB"}
                    },
                    ImageUrl = "images/items/IntelCorei3-9100F",
                    Description = "9th Gen Intel Core i3-9100f desktop processor without processor graphics."
                },

                new Item
                {
                    Title = "Intel Core i3-10100 Desktop Processor 4 Cores", Brand = "Intel", Price = 149.69,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Processor Speed", "4.3 GHz"},
                        {"Processor Socket", "LGA 1200"},
                        {"TDP", "85W"},
                        {"Smart Cache", "9MB"}
                    },
                    ImageUrl = "images/items/IntelCorei3-10100",
                    Description = "10th Gen Intel Core i3-10100 desktop processor optimized for everyday computing. Cooler included in the box. ONLY compatible with 400 series chipset based motherboard. 65W."
                },

                new Item
                {
                    Title = "Intel Core i3-8100 Desktop Processor 4", Brand = "Intel", Price = 123.80,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Processor Speed", "3.6 GHz"},
                        {"Processor Socket", "LGA 1156"},
                        {"UHD Graphics", "630"},
                        {"Smart Cache", "6MB"}
                    },
                    ImageUrl = "images/items/IntelCorei3-8100",
                    Description = "Intel Core i3-8100 Desktop Processor 4 Cores up to 3.6 GHz Turbo Unlocked LGA1151 300 Series 95W"
                },

                new Item
                {
                    Title = "AMD Ryzen 3 3200G 4-Core", Brand = "AMD", Price = 130,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Processor Speed", "4.0 GHz"},
                        {"Processor Socket", "Socket AM4"},
                        {"TDP", "65W"},
                        {"Cache", "6MB"}
                    },
                    ImageUrl = "images/items/AMDRyzen33200G",
                    Description = "AMD Ryzen 3 3200G, With Wraith Stealth C."
                },
                new Item
                {
                    Title = "Intel Core i7-9700 Desktop Processor 8 Cores", Brand = "Intel", Price = 292.60,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Processor Speed", "3.0 GHz"},
                        {"Processor Socket", "LGA 1151"},
                        {"TDP", "75W"},
                        {"Cache", "9MB"}
                    },
                    ImageUrl = "images/items/IntelCorei7-9700",
                    Description = "9th Gen Intel Core i7-9700 desktop processor with Intel Turbo Boost Technology 2.0 and Intel vPro Technology."
                },
                new Item
                {
                    Title = "AMD Ryzen 7 3700X 8-Core", Brand = "AMD", Price = 304.99,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Processor Speed", "4.4 GHz"},
                        {"Processor Socket", "Socket AM4"},
                        {"TDP", "65W"},
                        {"Cache", "6MB"}
                    },
                    ImageUrl = "images/items/AMDRyzen73700X",
                    Description = "AMD Ryzen 7 3700X 8 core, 16 thread unlocked desktop processor with Wraith Prism LED cooler. Base Clock - 3.6GHz.Default TDP / TDP: 65W."
                },
                //START HERE.................
                new Item
                {
                    Title = "Intel Core i5-9400 Desktop Processor 6 Cores", Brand = "Intel", Price = 164.70,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Processor Speed", "2.9 GHz"},
                        {"Processor Socket", "LGA 1151"},
                        {"TDP", "75W"},
                        {"Cache", "9MB"}
                    },
                    ImageUrl = "images/items/IntelCorei5-9400",
                    Description = "Intel Core i5-9400 Desktop Processor 6 Cores 2. 90 GHz up to 4. 10 GHz Turbo LGA1151 300 Series 65W Processors BX80684I59400"
                },

                new Item
                {
                    Title = "Intel Core i9-9900K Desktop Processor 8 Cores", Brand = "Intel", Price = 399.99,
                    Category = context.Category.Find(2), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Processor Speed", "5.0 GHz"},
                        {"Processor Socket", "LGA 1151"},
                        {"TDP", "95W"},
                        {"Cache", "12MB"}
                    },
                    ImageUrl = "images/items/IntelCorei9-9900K",
                    Description = "Intel Core i9-9900K Desktop Processor 8 Cores up to 5.0 GHz Turbo unlocked LGA1151 300 Series 95W"
                },
                new Item
                {
                    Title = "AOC C24G1A 24 Curved Frameless Gaming Monitor, FHD 1920x1080", Brand = "AOC", Price = 399.99,
                    Category = context.Category.Find(5), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Display Size", "24 Inches"},
                        {"Resolution", "FHD 1920x1080"},
                        {"Hardware Interface", "VGA, HDMI"},
                        {"Display Technology", "LED"}
                    },
                    ImageUrl = "images/items/AOC_black_monitor",
                    Description = "AOC CQ32G1 31.5 Curved Frameless Gaming Monitor, Quad HD 2560x1440 ,4 ms Response Time, 144Hz, FreeSync, DisplayPort/HDMI/VGA, VESA, Black"
                },

                new Item
                {
                    Title = "Hbada Gaming Chair Racing", Brand = "Hbada", Price = 159.99,
                    Category = context.Category.Find(6), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Chair Style", "Gaming chair"},
                        {"Headrest Features", "Padded"},
                        {"Swivel Angle", "360 degrees"},
                        {"Color", "Black"}
                    },
                    ImageUrl = "images/items/Hbada_Gaiming_Chair_Style",
                    Description = "High Back Computer Chair with Height Adjustment, Headrest and Lumbar Support E-Sports Swivel Chair"
                },

                new Item
                {
                    Title = "PHILIPS RGB Wired Gaming Mouse", Brand = "PHILIPS", Price = 29.99,
                    Category = context.Category.Find(4), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Mouse Shape", "Right-Handed"},
                        {"Connection Type", "Wired"},
                        {"Weight", "56g"},
                        {"Water Resistant", "No"}
                    },
                    ImageUrl = "images/items/PHILIPS_Gaming_MICE",
                    Description = "Gaming Mouse, 7 Programmable Buttons, Adjustable DPI, Comfortable Grip Ergonomic Optical PC Computer Gamer Mice"
                },
                new Item
                {
                    Title = "Corsair Harpoon PRO", Brand = "Corsair", Price = 49.99,
                    Category = context.Category.Find(4), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Mouse Shape", "Right-Handed"},
                        {"Connection Type", "Wired"},
                        {"Weight", "85g"},
                        {"Water Resistant", "yes"}
                    },
                    ImageUrl = "images/items/Corsair_mice",
                    Description = "RGB Gaming Mouse - Lightweight Design - 12,000 DPI Optical Sensor"
                },
                new Item
                {
                    Title = "AORUS Gaming Monitor 240Hz 1080P Adaptive Sync",
                    Brand = "AORUS",
                    Price = 429.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/AORUSFI25F24.5240Hz1080PAdaptiveSyncGami",
                    Description = "FHD WITH 240HZ Supports Adaptive-Sync (FreeSync Premium) Technology 24.5” FHD panel (1920 x 1080 resolution)"
                },
                new Item
                {
                    Title = "Asus Gaming Monitor Full HD 1920 x 1080",
                    Brand = "Asus",
                    Price = 344.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ASUSTUFGamingVG279QM27FullHD1920x10801ms",
                    Description = "1 ms 280Hz (Overclocking)"
                },
                new Item
                {
                    Title = "Acer Gaming Monitor 27 inch QHD 2560 x 1440",
                    Brand = "Acer",
                    Price = 211.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/AcerKA272UbiipxUM.HX2AA.00427QHD2560x144",
                    Description = "DisplayPort AMD RADEON FreeSync Technology Gaming Monitor"
                },
                new Item
                {
                    Title = "MSI Gaming Monitor 3440 x 1440 UWHD 144Hz",
                    Brand = "MSI",
                    Price = 689.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/MSIOptixMPG341CQR343440x1440UWHD144Hz1ms",
                    Description = "1ms 2xHDMI DisplayPort USB Type-C AMD FreeSync HDR"
                },
                new Item
                {
                    Title = "Samsung Gaming Monitor 3840 x 1080 1ms 144Hz",
                    Brand = "Samsung",
                    Price = 789.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/SamsungCHG90SeriesC49HG90493840x10801ms1",
                    Description = "2x HDMI DisplayPort Mini-DisplayPort HDR AMD FreeSync USB"
                },
                new Item
                {
                    Title = "MSI GeForce GTX 1660 Ti",
                    Brand = "MSI",
                    Price = 242.99,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIGeForceGTX1660TiDirectX12GTX1660TIVEN",
                    Description = "DirectX 12 GTX 1660 TI VENTUS XS 6G OC 6GB 192-Bit GDDR6"
                },
                new Item
                {
                    Title = "GIGABYTE Radeon RX 5700 ",
                    Brand = "GIGABYTE",
                    Price = 368.99,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/GIGABYTERadeonRX5700XTDirectX12GV-R57XTG",
                    Description = "Powered by AMD Radeon 8GB 256-Bit GDDR6"
                },
                new Item
                {
                    Title = "MSI GeForce GTX 1050 Ti",
                    Brand = "MSI",
                    Price = 138.65,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIGeForceGTX1050TiDirectX12GTX1050TiGAM",
                    Description = "GAMING X 4G 4GB 128-Bit GDDR5"
                },
                new Item
                {
                    Title = "Intel Core i9-10900K",
                    Brand = "Intel",
                    Price = 484.87,
                    Category = context.Category.Find(2),
                    ImageUrl = "images/items/IntelCorei9-10900K10-Core3.7GHzLGA120012",
                    Description = "10th Gen Intel Core Desktop Processor"
                },
                new Item
                {
                    Title = "Corsair Gaming Mouse M65 RGB ELITE",
                    Brand = "Corsair",
                    Price = 54.89,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/CorsairM65RGBELITETunableFPSGamingMouse,",
                    Description = "gaming mouse takes your FPS gameplay to the next level"
                },
                new Item
                {
                    Title = "Glorious Glossy Black Gaming Mouse",
                    Brand = "Glorious ",
                    Price = 36.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/GloriousModelOMinusGOM-GBLACKGlossyBlack",
                    Description = "58mm width at grip, 63mm with at back, 58mm at front, 120mm long"
                },

                new Item
                {
                    Title = "Corsair Gaming Headset HS70",
                    Brand = "Corsair",
                    Price = 45.55,
                    Category = context.Category.Find(7),
                    ImageUrl = "images/items/CorsairHS70PROWIRELESSUSBConnectorCircum",
                    Description = "Play with the freedom of up to 40ft of wireless range and up to 16 hours of battery life"
                },

                new Item
                {
                    Title = "Mad Catz Gaming Headset 7.1",
                    Brand = "MadCatz",
                    Price = 41.45,
                    Category = context.Category.Find(7),
                    ImageUrl = "images/items/MadCatzF.R.E.Q.4USBConnectorCircumauralG",
                    Description = "Fully immerse yourself in your game with dynamic, full-range sound pumped from super-sized 50mm neodymium drivers. Its 7.1 virtual sound lets you hear directional movement to gain a competitive edge, and its noise-canceling mic delivers crystal-clear audio without annoying background noise. This USB gaming headset also features brilliant RGB lighting, ergonomic comfort, and easy-access inline control for volume adjustment and mic mute."
                },

                new Item
                {
                    Title = "Corsair Premium Gaming Headset",
                    Brand = "Corsair",
                    Price = 49.55,
                    Category = context.Category.Find(7),
                    ImageUrl = "images/items/CorsairGamingVOIDPRORGBWirelessPremiumGa",
                    Description = "Maximum audio performance with DTS Headphone: X 7.1 Surround for "
                },

                new Item
                {
                    Title = "Logitech Wired Gaming Headset ",
                    Brand = "Logitech",
                    Price = 39.89,
                    Category = context.Category.Find(7),
                    ImageUrl = "images/items/Logitech981-000681G4337.1WiredGamingHead",
                    Description = "Extremely lightweight for maximum comfort"
                },

                new Item
                {
                    Title = "Razer Huntsman Elite Gaming Keyboard",
                    Brand = "Razer",
                    Price = 220.45,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/RazerHuntsmanEliteOpto-MechanicalSwitch",
                    Description = "The New Razer Opto-Mechanical Switch - Actuation at the speed of light"
                },

                new Item
                {
                    Title = "Razer Pokemon Pikachu Edition Gaming Keyboard",
                    Brand = "Razer",
                    Price = 199.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/RazerPokemonPikachuEditionGamingKeyboard",
                    Description = "This is the special Pokemon Pikachu Edition China Exclusive by Razer."
                },

                new Item
                {
                    Title = "Razer Overwatch Razer BlackWidow Chroma Mechanical Gaming Keyboard",
                    Brand = "Razer",
                    Price = 239.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/RazerOverwatchRazerBlackWidowChromaMecha",
                    Description = "Chroma customizable backlighting - With 16.8 Million color options,Exclusive Overwatch design"
                },

                new Item
                {
                    Title = "Razer Ornata Expert Gaming Keyboard",
                    Brand = "Razer",
                    Price = 119.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/RazerOrnataExpert-RevolutionaryMecha-Mem",
                    Description = "Designed from the ground up, the all-new Razer Mecha-Membrane combines the soft cushioned touch of a membrane rubber dome with the crisp tactile click of a mechanical switch"
                },

                new Item
                {
                    Title = "Razer BlackWidow X Tournament Edition Chroma Gaming Keyboard",
                    Brand = "Razer",
                    Price = 229.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/RazerBlackWidowXTournamentEditionChroma-",
                    Description = "Express your individuality and get the leg-up in games with Chroma backlighting and over 16.8 million color options"
                },

                new Item
                {
                    Title = "KLIM Puma - USB Gamer Headset with Mic", Brand = "KlimPuma", Price = 44.99,
                    Category = context.Category.Find(7), PropertiesList = new Dictionary<string, string>()
                    {
                        {"Sound Mode", "Stereo"},
                        {"Connection Type", "Wired"},
                        {"Water Resistant", "Yes"}
                    },
                    ImageUrl = "images/items/KLIM_headset",
                    Description = "7.1 Surround Sound Audio - Integrated Vibrations - Perfect for PC and PS4 Gaming - New 2020 Version - Black"
                },
                new Item
                {
                    Title = "GeForce RTX 3090 DirectX 12 RTX",
                    Brand = "MSI",
                    Price = 459.99,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIGeForceRTX3090DirectX12RTX3090VENTUS3",
                    Description = "DirectX 12 RTX 3090 VENTUS 3X 24G OC 24GB 384-Bit GDDR6X PCI Express 4.0 HDCP Ready SLI Support Video Card"
                },


                new Item
                {
                    Title = "Logitech Black Blueto Gaming Mouse",
                    Brand = "Logitech",
                    Price = 83.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/LogitechMXAnywhere3910-005987BlackBlueto",
                    Description = "USB-C quick charging - up to 70 days of power per full charge; up to 3 hours of power with one-minute charge"
                },
                new Item
                {
                    Title = "Razer DeathAdder RZ01 Gaming Mouse",
                    Brand = "Razer",
                    Price = 25.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/RAZERDeathAdderRZ01-00151400-R3U1BlackWi",
                    Description = "Black Wired Optical Precision Gaming Mouse - 3.5G Infrared Sensor"
                },
                new Item
                {
                    Title = "Creative Gaming Headset EV03",
                    Brand = "Creative",
                    Price = 99.99,
                    Category = context.Category.Find(7),
                    ImageUrl = "images/items/CreativeSoundBlasterEVO3.5mmUSBConnector",
                    Description = "Meet the Sound Blaster EVO, a headset that's amazing for audio"
                },
                new Item
                {
                    Title = "Asus Gaming Headset H5",
                    Brand = "Asus",
                    Price = 115.65,
                    Category = context.Category.Find(7),
                    ImageUrl = "images/items/ASUSTUFGamingH53.5mmUSBConnectorCircumau",
                    Description = "USB 2.0 or 3.5mm connector gaming headset with Onboard 7.1 Virtual Surround Sound"
                },
                new Item
                {
                    Title = "Creative Gaming Headset HS-720",
                    Brand = "Creative",
                    Price = 46.99,
                    Category = context.Category.Find(7),
                    ImageUrl = "images/items/CreativeChatMaxHS-72051EF0410AA004USBCon",
                    Description = "USB Connector USB Headset for Online Chats and PC Gaming"
                },
                new Item
                {
                    Title = "Logitech Gaming Headset H820e ",
                    Brand = "Logitech",
                    Price = 209.99,
                    Category = context.Category.Find(7),
                    ImageUrl = "images/items/LogitechH820eUSBConnectorSupra-auralWire",
                    Description = "USB Connector Supra-aural Wireless Headset"
                },
                new Item
                {
                    Title = "Logitech Gaming Headset G533",
                    Brand = "Logitech",
                    Price = 59.99,
                    Category = context.Category.Find(7),
                    ImageUrl = "images/items/LogitechG533WirelessDTS7.1SurroundSoundG",
                    Description = "Wireless DTS 7.1 Surround Sound"
                },
                new Item
                {
                    Title = "Razer Battlefield 4 BlackWidow Gaming Keyboard",
                    Brand = "Razer",
                    Price = 699.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/RazerBattlefield4BlackWidowUltimateMecha",
                    Description = "Never miss a key in the dark with backlit keys that give you the tactical edge, allowing you to launch assaults and flank your foes even under low light conditions."
                },
                new Item
                {
                    Title = "Logitech Gaming Keyboard G613",
                    Brand = "Logitech",
                    Price = 109.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/LogitechG613WirelessMechanicalGamingKeyb",
                    Description = "Six programmable G-keys - Put custom macro sequences and in-app commands at your fingertips. Customize G-key profiles individually for each app"
                },

                new Item
                {
                    Title = "Corsair K55 Gaming Keyboard ",
                    Brand = "Corsair",
                    Price = 24.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/CorsairGamingK55RGBKeyboard,BacklitRGBLE",
                    Description = "6 programmable macro keys enable powerful actions, key remaps and combos"
                },
                new Item
                {
                    Title = "Razer Blackwidow Ultimate 2016 Gaming Keyboard",
                    Brand = "Razer",
                    Price = 389.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/RazerBlackwidowUltimate2016MechanicalGam",
                    Description = "Individually backlit keys with Dynamic lighting effects"
                },
                new Item
                {
                    Title = "Logitech G903 Lightspeed Gaming Mouse ",
                    Brand = "Logitech",
                    Price = 204.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/LogitechG903LIGHTSPEEDWirelessGamingMous",
                    Description = "Wireless Gaming Mouse with HERO 16K Sensor, 140+ Hour with Rechargeable Battery, LIGHTSYNC RGB, POWERPLAY"
                },
                new Item
                {
                    Title = "Logitech Lightspeed G604 Gaming Mouse",
                    Brand = "Logitech",
                    Price = 189.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/LogitechG604LIGHTSPEEDWirelessGamingMous",
                    Description = "15 Programmable Controls, Dual Wireless Connectivity Modes, and HERO 16K Sensor"
                },
                new Item
                {
                    Title = "Asus Gaming Monitor",
                    Brand = "Asus",
                    Price = 132.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ASUSVP247QG24(Actualsize23.6)FullHD1920x",
                    Description = "ASUS Eye Care technology lowers blue light and eliminates flickering to reduce eyestrain and ailments"
                },
                new Item
                {
                    Title = "Samsung T55 Gaming Monitor",
                    Brand = "Samsung",
                    Price = 199.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/SAMSUNGT55SeriesC32T5532(Actualsize31.5)",
                    Description = "Full HD 1920 x 1080 75 Hz VGA, HDMI, DisplayPort FreeSync (AMD Adaptive Sync) Curved Gaming Monitor"
                },
                new Item
                {
                    Title = "MSI Optix MAG273R Gaming Monitor",
                    Brand = "MSI",
                    Price = 209.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/MSIOptixMAG273R27FullHD1920x10801ms144Hz",
                    Description = "27 inch Full HD 1920 x 1080 1 ms 144 Hz HDMI, DisplayPort, USB FreeSync (AMD Adaptive Sync) IPS Gaming Monitor"
                },
                new Item
                {
                    Title = "MSI Optix MAG240CR Gaming Monitor",
                    Brand = "MSI",
                    Price = 214.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/MSIOptixMAG240CR24FullHD1920x10801ms(MPR",
                    Description = "24 Full HD DisplayPort FreeSync (AMD Adaptive Sync) Curved Gaming Monitor"
                },
                new Item
                {
                    Title = "MSI Optix MAG271VCR Gaming Monitor",
                    Brand = "MSI",
                    Price = 249.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/MSIOptixMAG271VCR27FullHD1920x10801ms(MP",
                    Description = "Wide Color Gamut - Game colors and details will look more realistic and refined, to push game immersion to its limits."
                },
                new Item
                {
                    Title = "Samsung Odyssey G9 Series C49G97T Gaming Monitor",
                    Brand = "Samsung",
                    Price = 1199.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/SAMSUNGOdysseyG9SeriesC49G97T49DualQHD51",
                    Description = "Screen resolution of 5120 x 1440 and a refresh rate of 240 Hz 1ms (GTG). Perfect for competitive gaming sessions"
                },
                new Item
                {
                    Title = "Samsung Odyssey G7 Series C32G75T Gaming Monitor",
                    Brand = "Samsung",
                    Price = 789.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/SAMSUNGOdysseyG7C32G75T32(ActualSize31.5",
                    Description = "2xDisplayPort, USB G-Sync Compatible Curved Gaming Monitor"
                },
                new Item
                {
                    Title = "GIGABYTE G34WQC Gaming Monitor",
                    Brand = "GIGABYTE",
                    Price = 239.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/GIGABYTEG34WQC34144HzCurvedGamingMonitor",
                    Description = "GIGABYTE Gaming monitor features an exclusive stand that's ergonomically designed to offer extensive range of height and tilt adjustments."
                },
                new Item
                {
                    Title = "GIGABYTE G32QC Gaming Monitor",
                    Brand = "GIGABYTE",
                    Price = 129.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/GIGABYTEG32QC32165Hz1440PCurvedGamingMon",
                    Description = "Ergonomic Design with Tilt and Height Adjustments"
                },
                new Item
                {
                    Title = "GIGABYTE G27F Gaming Monitor",
                    Brand = "GIGABYTE ",
                    Price = 245.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/GIGABYTEG27F27144Hz1080PGamingMonitor,19",
                    Description = "High resolution and fast refresh rate, giving you the detailed display quality and fluid gaming experience!"
                },
                new Item
                {
                    Title = "GIGABYTE G27FC Gaming Monitor",
                    Brand = "GIGABYTE",
                    Price = 245.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/GIGABYTEG27FC27165Hz1080PCurvedGamingMon",
                    Description = "Smooth Gameplay with AMD FreeSync Premium"
                },
                new Item
                {
                    Title = "LG 32UK550-B Black Gaming Monitor ",
                    Brand = "LG",
                    Price = 329.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/LG32UK550-BBlack324ms(GTG)HDMIWidescreen",
                    Description = "32 Inch 4K UHD"
                },
                new Item
                {
                    Title = "Acer KG221Q Gaming Monitor",
                    Brand = "Acer",
                    Price = 99.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/AcerKG221QAbmix22(Actualsize21.5)1ms(GTG",
                    Description = "AMD FreeSync Technology 24', ZeroFrame design, Acer Flicker-less technology and Acer BlueLightShield technology"
                },
                new Item
                {
                    Title = "LG 24MP59G-P Gaming Monitor",
                    Brand = "LG",
                    Price = 209.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/LG24MP59G-P24(Actualsize23.8)FullHD1920x",
                    Description = "Full HD 1920 x 1080 5ms 75Hz VGA HDMI AMD FreeSync Flicker Safe Anti-Glare Backlit LED IPS Gaming Monitor"
                },
                new Item
                {
                    Title = "Asus ROG Gaming Monitor",
                    Brand = "Asus",
                    Price = 699.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ASUSROGStrixXG32VQ32(Actualsize31.5)WQHD",
                    Description = "32' (Actual size 31.5') WQHD 2560 x 1440 2K Adaptive/FreeSync 144Hz 4ms Curved Gaming Monitors with Aura RGB Lighting Asus Eye Care with Ultra Low-Blue Light & Flicker-Free"
                },
                new Item
                {
                    Title = "LG 34UM69G-B Gaming Monitor",
                    Brand = "LG",
                    Price = 314.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/LG34UM69G-B34FullHD2560x108075Hz5ms(GTG)",
                    Description = "34' Full HD 2560 x 1080 75Hz 5ms (GTG) DisplayPort USB Type-C AMD FreeSync Built-in Speakers Flicker Safe Ultrawide Backlit LED IPS Gaming Monitor"
                },
                new Item
                {
                    Title = "Corsair MM200 PRO Gaming Mouse Pad",
                    Brand = "Corsair",
                    Price = 24.99,
                    Category = context.Category.Find(8),
                    ImageUrl = "images/items/CorsairMM200PROCH-9412660-WWPremiumSpill",
                    Description = "Spill-Proof Cloth Gaming Mouse Pad - Heavy XL, Black"
                },
                new Item
                {
                    Title = "Corsair MM300 PRO Gaming Mouse Pad",
                    Brand = "Corsair",
                    Price = 22.99,
                    Category = context.Category.Find(8),
                    ImageUrl = "images/items/CorsairMM300PROCH-9413631-WWPremiumSpill",
                    Description = "A spill-proof and stain-resistant coating makes liquids slide right off the surface so your mouse pad is easy to wipe clean even after an accident."
                },
                new Item
                {
                    Title = "Corsair MM200 Medium Cloth Gaming Mouse Pad",
                    Brand = "Corsair",
                    Price = 12.99,
                    Category = context.Category.Find(8),
                    ImageUrl = "images/items/CorsairGamingMM200MediumClothGamingMouse",
                    Description = "Anti-skid rubber base helps it stay securely in place"
                },
                new Item
                {
                    Title = "Corsair MM300 Anti-Fray Gaming Mouse Pad",
                    Brand = "Corsair",
                    Price = 26.99,
                    Category = context.Category.Find(8),
                    ImageUrl = "images/items/CorsairGamingMM300Anti-FrayClothGamingMo",
                    Description = "Superior control, textile weave for pixel-precise targeting, low friction tracking"
                },
                new Item
                {
                    Title = "Corsair MM350 Gaming Mouse Pad",
                    Brand = "Corsair",
                    Price = 29.99,
                    Category = context.Category.Find(8),
                    ImageUrl = "images/items/CorsairMM350PROCH-9413770-WWPremiumSpill",
                    Description = "An anti-skid textured rubber base keeps the mouse pad securely in place even during the most intense gaming sessions"
                },
                new Item
                {
                    Title = "Asus ROG Sheath Electro Punk with Extra-large Gaming Mouse Pad",
                    Brand = "Asus",
                    Price = 31.99,
                    Category = context.Category.Find(8),
                    ImageUrl = "images/items/ROGSheathElectroPunkwithExtra-large,Gami",
                    Description = "Ultra-smooth surface for pixel-precise tracking"
                },
                new Item
                {
                    Title = "Logitech M317c Mouse",
                    Brand = "Logitech",
                    Price = 89.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/LogitechM317cCollectionWirelessMouseM317",
                    Description = "Electronics Features: Wireless, softtouch grips, scrolling wheel System Requirements: USB Port Operating System"
                },
                new Item
                {
                    Title = "Asus UT220 Ergonomic Design Mouse",
                    Brand = "Asus",
                    Price = 119.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/AsusUT220ErgonomicDesign,ClassicExterior",
                    Description = " Classic Exterior Extendable Cable Wired Mouse For Office And Game, High Compatibility Support PC, and Laptop-Black"
                },
                new Item
                {
                    Title = "MSI CLUTCH GM30 Gaming Mouse ",
                    Brand = "MSI",
                    Price = 49.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/MSICLUTCHGM306Buttons1xWheelUSB2.0WiredO",
                    Description = "6 Buttons 1 x Wheel USB 2.0 Wired Optical 6200 dpi Gaming Mouse"
                },
                new Item
                {
                    Title = "Logitech G203 Gaming Mouse",
                    Brand = "Logitech",
                    Price = 51.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/LogitechG203LIGHTSYNC910-005791White6But",
                    Description = "LIGHTSYNC 910-005791 White 6 Buttons 1 x Wheel USB Wired 8000 dpi Gaming Mouse"
                },
                new Item
                {
                    Title = "MSI Clutch GM11 Gaming Mouse",
                    Brand = "MSI",
                    Price = 48.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/MSIClutchGM11Black6Buttons1xWheelUSB2.0W",
                    Description = "Black 6 Buttons 1 x Wheel USB 2.0 Wired Optical 5000 dpi Gaming Mouse"
                },
                new Item
                {
                    Title = "Razer Naga Chroma - Multi-color MMO Gaming Mouse",
                    Brand = "Razer",
                    Price = 729.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/RazerNagaChroma-Multi-colorMMOGamingMous",
                    Description = "Chroma lighting with 16.8 customizable color options"
                },
                new Item
                {
                    Title = "Razer Lancehead Wireless Gaming Mouse",
                    Brand = "Razer",
                    Price = 239.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/RazerLancehead-ProfessionalGradeChromaAm",
                    Description = "Gaming-grade wireless performance -16000 dpi"
                },
                new Item
                {
                    Title = "Razer Mamba Tournament Edition Chroma Gaming Mouse",
                    Brand = "Razer",
                    Price = 259.99,
                    Category = context.Category.Find(4),
                    ImageUrl = "images/items/RAZERMambaTournamentEditionChromaGamingM",
                    Description = "Nine independently programmable buttons with tilt-click scroll"
                },
                new Item
                {
                    Title = "GIGABYTE G27QC 27' 165Hz 1440P Curved Gaming Monitor",
                    Brand = "GIGABYTE",
                    Price = 349.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/GIGABYTEG27QC27165Hz1440PCurvedGamingMon",
                    Description = "Super fast 1ms response time for the most smooth gaming experience ever!"
                },
                new Item
                {
                    Title = "Acer Nitro XZ322Q Pbmiiphx 31.5' Gaming Monitor",
                    Brand = "Acer",
                    Price = 369.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/AcerNitroXZ322QPbmiiphx31.5FULLHD165Hz1m",
                    Description = "FULL HD 165Hz 1ms FreeSync 1500R HDMI DP Build-in-Speaker HDR400 Curved Gaming Monitor"
                },
                new Item
                {
                    Title = "ViewSonic ELITE XG270Q 27' Gaming Monitor",
                    Brand = "ViewSonic",
                    Price = 439.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ViewSonicELITEXG270Q271ms1440p165HzG-SYN",
                    Description = "NVIDIA G-SYNC: Experience uninterrupted gaming with synchronized frame rates, variable overdrive, and ultra-low motion blur"
                },
                new Item
                {
                    Title = "ViewSonic VX3268-PC-MHD 32' 1080p Curved Gaming Monitor",
                    Brand = "ViewSonic",
                    Price = 259.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ViewSonicVX3268-PC-MHD321080pCurved165Hz",
                    Description = " FreeSync Premium Eye Care HDMI and Display Port 165Hz 1ms"
                },
                new Item
                {
                    Title = "ViewSonic XG2560 Gaming Monitor",
                    Brand = "ViewSonic",
                    Price = 444.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ViewSonicXG256025(Actualsize24.5)FullHD1",
                    Description = "Full HD 1920 x 1080 240Hz 1ms HDMI DisplayPort USB 3.0 Hub Built-in Speakers NVIDIA G-Sync Technology Anti-Glare Backlit LED Esports Gaming Monitor"
                },
                new Item
                {
                    Title = "ViewSonic VX2458-MHD 24' Gaming Monitor ",
                    Brand = "ViewSonic",
                    Price = 145.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ViewSonicVX2458-MHD24FullHD1920x10801ms1",
                    Description = "Full HD 1920 x 1080 1ms 144Hz 2 x HDMI DisplayPort FreeSync Built-in Speakers Anti-Glare LED Backlit LCD Gaming Monitor"
                },
                new Item
                {
                    Title = "ViewSonic VX3211-4K-MHD 32' Gaming Monitor",
                    Brand = "ViewSonic",
                    Price = 299.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ViewSonicVX3211-4K-MHD32UltraHD3840x2160",
                    Description = "Ultra HD 3840 x 2160 4K 2xHDMI DisplayPort Built-in Speakers AMD FreeSync Technology Blue Light Filter Flicker-Free HDR10 Compatible Backlit LED Gaming Monitor"
                },
                new Item
                {
                    Title = "ViewSonic XG2405 24' Gaming Monitor",
                    Brand = "ViewSonic",
                    Price = 289.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ViewSonicXG240524(23.8Viewable)FullHD192",
                    Description = "Full HD 1920 x 1080 1ms (GTG) 144Hz HDMI, DisplayPort IPS AMD FreeSync Built-in Speakers Gaming"
                },
                new Item
                {
                    Title = "ViewSonic XG2760 27' Gaming Monitor",
                    Brand = "ViewSonic",
                    Price = 699.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ViewSonicXG276027QuadHD2560x14402KFastAc",
                    Description = "Quad HD 2560 x 1440 2K Fast Action 165Hz 1ms HDMI DisplayPort NVIDIA G-Sync USB Hub Backlit LED Anti-Glare Gaming Monitor"
                },
                new Item
                {
                    Title = "ViewSonic Curved 32' 144Hz Gaming Monitor ",
                    Brand = "ViewSonic",
                    Price = 239.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ViewSonicVX3268-2KPC-MHD32QHD1440pCurved",
                    Description = "Equipped with ViewSonic exclusive ViewMode presets, which offer optimized screen performance for different home entertainment applications"
                },
                new Item
                {
                    Title = "ViewSonic LED Curved 27' Gaming Monitor",
                    Brand = "ViewSonic",
                    Price = 349.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ViewSonicXG270QC27WQHD2560x14402K165Hz1m",
                    Description = "WQHD 2560 x 1440 2K 165Hz 1ms 2xHDMI DisplayPort AMD FreeSync Built-in Speakers USB 3.2 Hub Anti-Glare Backlit LED Curved Gaming Monitor"
                },
                new Item
                {
                    Title = "ViewSonic VX2252MH 22' Gaming Monitor",
                    Brand = "ViewSonic",
                    Price = 209.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ViewSonicVX2252MH22FullHD1920x10802ms(GT",
                    Description = "Full HD 1920 x 1080 2ms HDMI VGA DVI-D Built-in Speakers Anti-Glare Backlit LED Gaming Monitor"
                },
                new Item
                {
                    Title = "Asus ROG Strix XG43VQ 43' Gaming Monitor",
                    Brand = "Asus",
                    Price = 709.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ASUSROGStrixXG43VQ433840x12001msMPRT120H",
                    Description = "3840 x 1200 1ms 120 Hz HDMI, DisplayPort Built-in Speakers 1800R Curved Gaming Monitor with FreeSync 2 HDR, DisplayHDR 400, DCI-P3 90%, Shadow Boost"
                },
                new Item
                {
                    Title = "Asus ROG Strix XG17AHP 17.3' Gaming Monitor",
                    Brand = "Asus",
                    Price = 679.99,
                    Category = context.Category.Find(5),
                    ImageUrl = "images/items/ASUSROGStrixXG17AHP17.3FullHD1920x10803m",
                    Description = "Full HD 1920 x 1080 3 ms 240Hz ,USB Type-C, Micro HDMI Built-in Speakers Portable IPS Gaming Monitor"
                },
                new Item
                {
                    Title = "Logitech G513 Mechanical Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 129.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/LogitechG513RGBBacklitMechanicalGamingKe",
                    Description = "High performance RGB keyboard with customizable full spectrum color lighting per key plus LIGHTSYNC game-driven lighting colors and effects"
                },
                new Item
                {
                    Title = "Logitech G915 Mechanical Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 359.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/LogitechG915LightspeedWirelessRGBMechani",
                    Description = "IGHTSPEED wireless delivers pro-grade performance with flexibility and freedom from cords. Creates a clean aesthetic for battlestations."
                },
                new Item
                {
                    Title = "Logitech G915 Clicky Switch  Mechanical Gaming Keyboard",
                    Brand = "Logitech ",
                    Price = 239.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/Logitech920-009529G915TenkeylessLIGHTSPE",
                    Description = "LIGHTSPEED wireless delivers pro-grade performance with flexibility and freedom from cords. Creates a clean aesthetic for battlestations. Delivers 40 hours on a single full charge."
                },
                new Item
                {
                    Title = "Logitech G413 Mechanical Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 179.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/LogitechG413BacklitMechanicalGamingKeybo",
                    Description = "Additional USB cable connects the USB pass-through port to its own input for 100% power throughout and data speed. Plug in a device to charge or plug in a mouse to charge your adversaries"
                },
                new Item
                {
                    Title = "Logitech G910 Mechanical Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 539.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/LogitechG910OrionSparkRGBMechanicalGamin",
                    Description = "World's fastest RGB mechanical gaming keyboard: Exclusive Romer-G Mechanical Switches with up to 25 percent faster actuation"
                },
                new Item
                {
                    Title = "Logitech G915 Lightspeed Wireless RGB Mechanical Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 439.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/LogitechG915LightspeedWirelessRGBMechani",
                    Description = "LIGHTSYNC technology provides next-gen RGB lighting that synchronizes lighting with any content. Personalize each key or create custom animations from ~16.8M colors with Logitech G HUB software."
                },
                new Item
                {
                    Title = "Logitech G910 ORION SPECTRUM RGB Mechanical Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 169.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/Logitech-G910ORIONSPECTRUMRGBMECHANICALG",
                    Description = "Personalize each key or create custom animations."
                },
                new Item
                {
                    Title = "Logitech G512 Carborn RGB Mechanical Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 359.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/logitechg512carbonrgbmechanicalgamingkey",
                    Description = "USB: Plug in your mouse, flash drive or phone with convenience via the integrated USB passthrough port"
                },
                new Item
                {
                    Title = "Logitech G512 N-Key RGB Mechanical Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 359.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/LogitechG512N-KeyRolloverErgonomicDesign",
                    Description = "Sync With Keyboard Light Effect and Gaming Sound Effect Through Logitech Gaming Software"
                },
                new Item
                {
                    Title = "Logitech K845 Wired Mechanical Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 359.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/LogitechK845WiredTTCMechanicalGamingKeyb",
                    Description = "104 Keys White Backlit And TTC Mechanical Switch For Windows/MAC/Android/IOS - Black"
                },
                new Item
                {
                    Title = "Logitech G213 Prodigy Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 25.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/LogitechG213ProdigyGamingKeyboardwith16",
                    Description = "Brilliant Color Spectrum Illumination lets you easily personalize up to 5 lighting zones from over 16.8 million colors to match your style and gaming gear"
                },
                new Item
                {
                    Title = "Logitech G610 Orion Brown Mechanical Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 429.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/LogitechG610OrionBrown,MechanicalGamingK",
                    Description = "Performance-driven gaming keyboard: Full-size keyboard delivering a pure, fluid gaming experience"
                },
                new Item
                {
                    Title = "Logitech G610 Orion Red Mechanical Gaming Keyboard",
                    Brand = "Logitech",
                    Price = 229.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/LogitechG610OrionRed,MechanicalGamingKey",
                    Description = "Customizable Lighting: Personalize individual key lighting brightness to keep track of spells and other commands"
                },
                new Item
                {
                    Title = "Corsair K63 Mechanical Gaming Keyboard",
                    Brand = "Corsair",
                    Price = 129.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/CorsairGamingK63CompactMechanicalKeyboar",
                    Description = "Dedicated volume and multimedia controls: Control to adjust audio on-the-fly without interrupting your game"
                },

                new Item
                {
                    Title = "Asus ROG Strix Flare RGB Mechanical Gaming Keyboard",
                    Brand = "Asus",
                    Price = 229.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/ASUSROGStrixFlareRGBMechanicalGamingKeyb",
                    Description = "Asus Aura Sync RGB lighting features a nearly endless spectrum of colors with the ability to synchronize effects across an ever-expanding ecosystem of Aura Sync enabled products"
                },
                new Item
                {
                    Title = "Asus ROG Strix Scope RGB Mechanical Gaming Keyboard",
                    Brand = "Asus",
                    Price = 129.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/ASUSROGStrixScopeTKLWiredMechanicalRGBGa",
                    Description = "Created for FPS Gamers: the tenkeyless form factor provides more room to move the mouse, while the enlarged L-Ctrl key minimizes inadvertent pressing of other keys"
                },
                new Item
                {
                    Title = "Asus TUF K7 Optical-mech Mechanical Gaming Keyboard",
                    Brand = "Asus",
                    Price = 229.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/ASUSTUFGamingK7Optical-mechGamingKeyboar",
                    Description = "ASUS Aura Sync RGB lighting features a nearly endless spectrum of colors with the ability to synchronize effects across an ever-expanding ecosystem of Aura Sync enabled products"
                },
                new Item
                {
                    Title = "Asus ROG Strix Scope TKL Electro Punk Gaming Keyboard",
                    Brand = "Asus",
                    Price = 129.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/ASUSX803STRIXSCOPETKLEPRDUSROGStrixScope",
                    Description = "Created for FPS Gamers: the tenkeyless form factor provides more room to move the mouse, while the enlarged L-Ctrl key minimizes inadvertent pressing of other keys"
                },
                new Item
                {
                    Title = "Asus ROG XA02 Mechanical Gaming Keyboard",
                    Brand = "Asus",
                    Price = 129.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/ASUSXA02ROGSTRIXSCOPEBNUSRGBMechanicalGa",
                    Description = "Created for FPS Gamers: the tenkeyless form factor provides more room to move the mouse, while the enlarged L-Ctrl key minimizes inadvertent pressing of other keys"
                },
                new Item
                {
                    Title = "Asus ROG Strix Scope Cherry MX Red Switches Mechanical Gaming Keyboard",
                    Brand = "Asus",
                    Price = 629.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/ASUSROGStrixScopeRGBMechanicalGamingKeyb",
                    Description = "Great for FPS games: Extra-wide, ergonomic Xccurate Ctrl key means fewer missed clicks for greater FPS precision"
                },
                new Item
                {
                    Title = "Asus STRIX TACTIC PRO Gaming Keyboard",
                    Brand = "Asus",
                    Price = 229.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/ASUSSTRIXTACTICPROGamingKeyboardwithCher",
                    Description = "Created for FPS Gamers: the tenkeyless form factor provides more room to move the mouse, while the enlarged L-Ctrl key minimizes inadvertent pressing of other keys"
                },
                new Item
                {
                    Title = "GIGABYTE AORUS K9 Optical RGB Gaming Keyboard",
                    Brand = "GIGABYTE",
                    Price = 329.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/GIGABYTEAORUSK9OpticalRGBGamingKeyboard-",
                    Description = "Swappable Switches - Custom Gaming Experience.Splash proof."
                },
                new Item
                {
                    Title = "Razer BlackWidow Tournament Edition Mechanical Gaming Keyboard",
                    Brand = "Razer",
                    Price = 629.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/RazerBlackWidowTournamentEditionChromaV2",
                    Description = "The new Razer Instant Trigger Technology immediately sends the signal to your system, cancelling any need for a denounce delay - With this, you get industry-leading speed for whatever you're playing"
                },
                new Item
                {
                    Title = "Razer RZ03 BlackWidow Ultimate Mass Effect 3 Edition Gaming Keyboard",
                    Brand = "Razer",
                    Price = 429.99,
                    Category = context.Category.Find(3),
                    ImageUrl = "images/items/RAZERRZ03-00381800-R3M1BlackWidowUltimat",
                    Description = "Created for FPS Gamers: the tenkeyless form factor provides more room to move the mouse, while the enlarged L-Ctrl key minimizes inadvertent pressing of other keys"
                },
                new Item
                {
                    Title = "MSI GeForce RTX 2070 SUPER DirectX 12 Video Card",
                    Brand = "MSI",
                    Price = 738.65,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIGeForceRTX2070SUPERDirectX12RTX2070Su",
                    Description = "Memory size 8GB 256-Bit GDDR6,Boost Clock 1800 MHz"
                },
                new Item
                {
                    Title = "MSI Radeon RX 580 DirectX 12 Video Card Video Card",
                    Brand = "MSI",
                    Price = 238.65,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIRadeonRX580DirectX12RX580ARMOR8GOC8GB",
                    Description = "8GB 256-Bit GDDR5,Boost Clock 1366 MHz"
                },
                new Item
                {
                    Title = "MSI GeForce GTX 1660 DirectX 12 Video Card",
                    Brand = "MSI",
                    Price = 438.99,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIGeForceGTX1660DirectX12GTX1660VENTUSX",
                    Description = "6GB 192-Bit GDDR5,Boost Clock 1830 MHz"
                },
                new Item
                {
                    Title = "MSI GeForce GTX 1660 TI ARMOR 6G OC 6GB Video Card",
                    Brand = "MSI",
                    Price = 538.65,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIGeForceGTX1660TiDirectX12GTX1660TIARM",
                    Description = "6GB 192-Bit GDDR6,PCI Express 3.0 x16"
                },
                new Item
                {
                    Title = "MSI GeForce RTX 2060 TRIO 6GB 192-Bit Video Card",
                    Brand = "MSI",
                    Price = 638.65,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIGeForceRTX2060DirectX12RTX2060GAMINGZ",
                    Description = "6GB 192-Bit GDDR6,Boost Clock 1830 MHz"
                },
                new Item
                {
                    Title = "MSI GeForce RTX 2080 Super Gaming Video Card",
                    Brand = "MSI",
                    Price = 1108.99,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIGeForceRTX2080SUPERDirectX12RTX2080Su",
                    Description = "8GB 256-Bit GDDR6 PCI Express 3.0 x16 HDCP Ready SLI Support Video Card"
                },
                new Item
                {
                    Title = "MSI GeForce GeForce GTX 1650 DirectX 12 Ready ITX Video Card",
                    Brand = "MSI",
                    Price = 238.65,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIGeForceGTX1650DirectX12GTX1650D6AEROI",
                    Description = "4GB 128-Bit GDDR6,Boost Clock 1620 MHz"
                },
                new Item
                {
                    Title = "MSI Radeon RX 5600 XT DirectX 12 Video Card",
                    Brand = "MSI",
                    Price = 438.65,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIRadeonRX5600XTDirectX12RX5600XTGAMING",
                    Description = "6GB 192-Bit GDDR6,Game Clock 1615 MHz"
                },
                new Item
                {
                    Title = "MSI GeForce RTX 3090 DirectX 12 Video Card",
                    Brand = "MSI",
                    Price = 2238.65,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/MSIGeForceRTX3090DirectX12RTX3090GAMINGX",
                    Description = "24GB 384-Bit GDDR6X,Boost Clock 1785 MHz"
                },
                new Item
                {
                    Title = "GIGABYTE GeForce RTX 3080 DirectX 12 Video Card",
                    Brand = "GIGABYTE",
                    Price = 738.65,
                    Category = context.Category.Find(1),
                    ImageUrl = "images/items/GIGABYTEGeForceRTX3080DirectX12GV-N3080G",
                    Description = "10GB 320-Bit GDDR6X,Boost Core Clock 1800 MHz"
                },
            };
            foreach (var item in items)
            {
                //item.Brand = item.Brand.Trim().FirstCharToUpper();
                context.Item.Add(item);
            }

            context.Item.AsNoTracking();
            context.SaveChanges();


            return items;
        }


        private static void test()
        {

        }
        //private static void GenerateStoreItems(IEnumerable<Store> stores, Item[] items, List<User> UsersList)
        //{
        //    var random = new Random();

        //    foreach (Store store in stores)
        //    {
        //        foreach (Item item in items)
        //        {
        //            bool itemCreated = random.Next(2) == 1; // 1 - True  - False

        //            if (!itemCreated)
        //            {
        //                continue;
        //            }

        //            const float itemsNumberMultiplier = 0.3f;

        //            store.StoreItems.Add(new StoreItem
        //            {
        //                Id = item.Id,
        //                StoreId = store.Id,
        //                ItemsCount =
        //                    (uint)random.Next(1,
        //                        (int)(UsersList.Count * itemsNumberMultiplier)) // Users number times 0.3
        //            });
        //        }
        //    }
        //}

        //private static IEnumerable<Order> GenerateOrders(IEnumerable<User> UsersList, IReadOnlyCollection<Item> items, IReadOnlyCollection<Store> storesList, out List<Payment> paymentsList)
        //{
        //    paymentsList = new List<Payment>();
        //    var orderList = new List<Order>();
        //    var rand = new Random();
        //    var shopOpeningDate = new DateTime(2018, 1, 1);
        //    int range = (DateTime.Today - shopOpeningDate).Days;

        //    List<User> regUsers = new List<User>();

        //    foreach (var User in UsersList)
        //    {
        //        var userRoles = _userManager.GetRolesAsync(User).Result;
        //        var UserRole = _roleManager.FindByNameAsync("User").Result;

        //        foreach (var role in userRoles)
        //        {
        //            if (UserRole.Name == role)
        //            {
        //                regUsers.Add(User);
        //            }
        //        }
        //    }

        //    try
        //    {
        //        foreach (User User in regUsers)
        //        {
        //            int numOfOrdersForUser = rand.Next(minValue: 0, maxValue: 5);

        //            for (var orderNumber = 0; orderNumber < numOfOrdersForUser; orderNumber++)
        //            {
        //                const int minItems = 1;
        //                const int maxItems = 5;
        //                int numItemsOrdered = rand.Next(minItems, maxItems);
        //                Store store = GenerateRelatedStore(User, storesList);

        //                var order = new Order
        //                {
        //                    UserId = User.Id,
        //                    OrderDate = shopOpeningDate.AddDays(rand.Next(range)),
        //                    State = OrderState.Fulfilled,
        //                    StoreId = store.Id,
        //                    Store = store
        //                };

        //                order.OrderItems = GenerateOrderItems(order.Id, items, numItemsOrdered, store, out Payment payment, out ShippingMethod shippingMethod);
        //                order.Payment = payment;
        //                order.Id = payment.Id;
        //                order.ShippingMethod = shippingMethod;
        //                paymentsList.Add(payment);
        //                orderList.Add(order);
        //            }
        //        }

        //        return orderList;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //}

        //private static Store GenerateRelatedStore(User User, IEnumerable<Store> stores)
        //{
        //    try
        //    {
        //        List<Store> storeList = stores.ToList();
        //        List<Store> storesInUserCity = storeList.Where(store => store.Address.City == User.Address.City).ToList();
        //        var rand = new Random();

        //        bool relatedStores = storesInUserCity.Count != 0;
        //        var randRelatedStoreIndex = rand.Next(relatedStores ? storesInUserCity.Count : storeList.Count);

        //        var generatedRelatedStore = relatedStores ? storesInUserCity[randRelatedStoreIndex] : storeList[randRelatedStoreIndex];

        //        return generatedRelatedStore;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //}

        //private static ICollection<OrderItem> GenerateOrderItems(string orderId, IEnumerable<Item> items, int numItemsOrdered, Store store, out Payment payment, out ShippingMethod shippingMethod)
        //{
        //    var itemsList = new List<Item>(items); // copy list in order to alter it.
        //    var rand = new Random();
        //    var orderItems = new List<OrderItem>();

        //    try
        //    {
        //        for (var orderItemIndex = 0; orderItemIndex < numItemsOrdered; orderItemIndex++)
        //        {
        //            int curIndex = rand.Next(itemsList.Count);
        //            Item curItem = itemsList[curIndex];
        //            itemsList.Remove(curItem);

        //            var orderItem = new OrderItem()
        //            {
        //                Id = orderId,
        //                Id = curItem.Id,
        //                Item = curItem,
        //                ItemsCount = rand.Next(1, 3)
        //            };

        //            orderItems.Add(orderItem);
        //        }

        //        double orderSum = CalculateOrderSum(orderItems);
        //        PaymentMethod paymentMethod;

        //        if (store.Name == "Website")
        //        {
        //            paymentMethod = (PaymentMethod)rand.Next(1, 2);
        //        }
        //        else
        //        {
        //            paymentMethod = (PaymentMethod)rand.Next(0, 2);
        //        }

        //        var r = new Random();
        //        var priceArray = new[] { 0, 10, 45 };
        //        int randomIndex = r.Next(priceArray.Length);
        //        int shippingCost = priceArray[randomIndex];

        //        shippingMethod = shippingCost switch
        //        {
        //            0 => ShippingMethod.Pickup,
        //            10 => ShippingMethod.Standard,
        //            45 => ShippingMethod.Express,
        //            _ => ShippingMethod.Other
        //        };

        //        payment = new Payment
        //        {
        //            ItemsCost = orderSum,
        //            PaymentMethod = paymentMethod,
        //            ShippingCost = shippingCost,
        //            Paid = true,
        //            Total = orderSum + shippingCost
        //        };

        //        return orderItems;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //}

        //private static double CalculateOrderSum(IEnumerable<OrderItem> orderItems)
        //{
        //    return orderItems.Sum(orderItem => (orderItem.Item.Price * (double)orderItem.ItemsCount));
        //}
    }
}