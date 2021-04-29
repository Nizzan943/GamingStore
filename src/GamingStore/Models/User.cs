using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Contracts;

namespace GamingStore.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<Order> OrdersHistory { get; set; }
        public List<Address> Addresses { get; set; }
        public string Password { get; set; }
        public DateTime JoinDate { get; set; }
    }
}
