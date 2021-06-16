using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Contracts;
using Microsoft.EntityFrameworkCore;
using GamingStore.Models;
using GamingStore.Models.Relationships;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GamingStore.Data
{
    public class GamingStoreContext : IdentityDbContext<User, IdentityRole, string>
    {
        public GamingStoreContext(DbContextOptions<GamingStoreContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Relationships

            #region OneToOne

            #endregion

            #region ManyToMany
            // StoreItem: Store 1..x-1..x Item
            modelBuilder.Entity<StoreItem>().HasKey(storeItem => new { storeItem.StoreId, storeItem.ItemId });
            modelBuilder.Entity<StoreItem>().HasOne(storeItem => storeItem.Store).WithMany(store => store.StoreItems)
                .HasForeignKey(storeItem => storeItem.StoreId);
            modelBuilder.Entity<StoreItem>().HasOne(storeItem => storeItem.Item).WithMany(item => item.StoreItems)
                .HasForeignKey(storeItem => storeItem.ItemId);
            #endregion

            #endregion

            #region ObjectConverationHandling

            modelBuilder.Entity<Item>().Property(i => i.PropertiesList).HasConversion(
                v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));
            modelBuilder.Entity<User>().Property(c => c.Address).HasConversion(v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Address>(v));
            modelBuilder
                .Entity<Address>(builder =>
                {
                    builder.HasNoKey();
                });

            modelBuilder.Entity<Store>().Property(c => c.Address).HasConversion(v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Address>(v));

            modelBuilder.Entity<Store>().Property(s => s.OpeningHours).HasConversion(
                v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<OpeningHours>>(v));

            modelBuilder.Entity<Order>().Property(c => c.ShippingAddress).HasConversion(v => JsonConvert.SerializeObject(v),
              v => JsonConvert.DeserializeObject<Address>(v));

            #endregion

            modelBuilder.Entity<OrderItem>().HasKey(orderItem => new { orderItem.OrderId, orderItem.ItemId });
            modelBuilder.Entity<OrderItem>().HasOne(orderItem => orderItem.Order).WithMany(order => order.OrderItems)
                .HasForeignKey(orderItem => orderItem.OrderId);
            modelBuilder.Entity<OrderItem>().HasOne(orderItem => orderItem.Item).WithMany(item => item.OrderItems)
                .HasForeignKey(orderItem => orderItem.ItemId);

            modelBuilder.Entity<Store>().Property(s => s.OpeningHours).HasConversion(
                v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<OpeningHours>>(v));

            //modelBuilder
            //    .Entity<Address>(builder =>
            //    {
            //        builder.HasNoKey();
            //    });

        }
        public DbSet<GamingStore.Models.Item> Item { get; set; }
        public DbSet<GamingStore.Models.Payment> Payment { get; set; }
        public DbSet<GamingStore.Models.User> User { get; set; }
        public DbSet<GamingStore.Models.Category> Category { get; set; }
        public DbSet<GamingStore.Models.Store> Store { get; set; }
        public DbSet<GamingStore.Models.Order> Order { get; set; }
        public DbSet<GamingStore.Models.Cart> Cart { get; set; }


    }
}