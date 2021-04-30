using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Contracts;
using Microsoft.EntityFrameworkCore;
using GamingStore.Models;
using Newtonsoft.Json;

namespace GamingStore.Data
{
    public class GamingStoreContext : DbContext
    {
        public GamingStoreContext(DbContextOptions<GamingStoreContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>().Property(i => i.PropertiesList).HasConversion(
                v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));
            modelBuilder.Entity<User>().Property(c => c.Address).HasConversion(v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Address>(v));
            modelBuilder
                .Entity<Address>(builder =>
                {
                    builder.HasNoKey();
                });

        }
        public DbSet<GamingStore.Models.Item> Item { get; set; }
        public DbSet<GamingStore.Models.Payment> Payment { get; set; }
        public DbSet<GamingStore.Models.User> User { get; set; }


    }
}
