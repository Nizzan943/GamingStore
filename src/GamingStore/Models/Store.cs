using GamingStore.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GamingStore.Models
{
    public class Store
    {
        public static int StoreCounter;

        public Store()
        {
            Orders = new List<Order>();
            StoreItems = new List<Item>();
            Id = StoreCounter;
            Interlocked.Increment(ref StoreCounter);
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required, DataType(DataType.Text), StringLength(50), RegularExpression(@"[a-zA-Z]{2,}$")]
        public string Name { get; set; }

        public Address? Address { get; set; }

        [DisplayName("Phone")]
        [Required, DataType(DataType.PhoneNumber), StringLength(50), Phone]
        public string PhoneNumber { get; set; }

        [Required, DataType(DataType.EmailAddress), EmailAddress]
        public string Email { get; set; }

        [DisplayName("Opening Hours")]
        public List<OpeningHours> OpeningHours { get; set; } = new List<OpeningHours>(7);

        //public Dictionary<Item,uint> Stock { get; set; } // determines how many items there are in the store. example: {{fridge, 5},{mouse,6}}
        public ICollection<Order> Orders { get; set; }

        public ICollection<Item> StoreItems { get; set; } // many to many relationship
        public bool Active { get; set; } = true;

        public bool IsOpen()
        {
            var currentDateTime = DateTime.Now;
            var dayOfWeek = (int)currentDateTime.DayOfWeek;
            var curTime = currentDateTime.TimeOfDay;

            return OpeningHours[dayOfWeek].OpeningTime <= curTime && curTime <= OpeningHours[dayOfWeek].ClosingTime;
        }
    }

}

