using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GamingStore.Models;
using Newtonsoft.Json;

namespace GamingStore.Data
{
    public class GamingStoreContext : DbContext
    {
        public GamingStoreContext (DbContextOptions<GamingStoreContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>().Property(i => i.PropertiesList).HasConversion(
                v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

            //modelBuilder.Entity<Item>().ToTable("Items");


        }
        public DbSet<GamingStore.Models.Item> Item { get; set; }


    }
}
