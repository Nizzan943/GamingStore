using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GamingStore.Models
{
    public class Item
    {

        [Key]
        public int ItemId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public float Price { get; set; }
        [Required]
        public string Brand { get; set; }
        [Required]
        public int StockCounter { get; set; }
        [Required]
        public string Description { get; set; }
        
        //public Dictionary<string,string> PropertiesList { get; set; }
        [Required]
        public string Category { get; set; }
        public float StarReview { get; set; }
        [Required]
        public string ImageUrl { get; set; }

        public bool Active { get; set; } = true;
    }
}
