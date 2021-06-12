using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Models.Relationships;

namespace GamingStore.Models
{
    public class Item
    {

        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
     
        public double Price { get; set; }
        [Required]
        public string Brand { get; set; }
        
        [Required]
        public string Description { get; set; }
        [Required, DisplayName("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        [NotMapped]
        public Dictionary<string,string> PropertiesList { get; set; } = new Dictionary<string, string>();
        [Required, DisplayName("Image URL")]
        public string ImageUrl { get; set; }

        public ICollection<StoreItem> StoreItems { get; set; } // many to many relationship

        public bool Active { get; set; } = true;
    }
}
