using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GamingStore.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [DataType(DataType.Text)]
        public string UserId { get; set; }

        public int Quantity { get; set; }

        [NotMapped]
        public Item Item { get; set; }
    }
}
