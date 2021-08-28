using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamingStore.Models
{
    public class Category
    {

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Image { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
    }
}
