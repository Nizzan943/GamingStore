using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GamingStore.Models;

namespace GamingStore.Data
{
    public class GamingStoreContext : DbContext
    {
        public GamingStoreContext (DbContextOptions<GamingStoreContext> options)
            : base(options)
        {
        }

        public DbSet<GamingStore.Models.Item> Item { get; set; }
    }
}
