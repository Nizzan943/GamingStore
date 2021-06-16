using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;

namespace GamingStore.Models
{
    public class Cart
    {
        public static int ItemCounter;

        public Cart()
        {
            Id = ItemCounter;
            Interlocked.Increment(ref ItemCounter);
        }

        [Key, DatabaseGenerated((DatabaseGeneratedOption.None))]
        public int Id { get; set; }

        [DataType(DataType.Text)]
        public string UserId { get; set; }

        [DataType(DataType.Currency)]
        public int ItemId { get; set; }

        [DataType(DataType.Currency)]
        [Range(1, 9, ErrorMessage = "only 1-9 items is allowed")]
        public int Quantity { get; set; }

        [NotMapped]
        public Item Item { get; set; }
    }
}