using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GamingStore.Contracts;
using Microsoft.AspNetCore.Identity;

namespace GamingStore.Models
{
    public class User : IdentityUser
    {
        public User()
        {
            OrdersHistory = new List<Order>();
        }


        [Required]
        [Display(Name = "First Name")]
        [DataType(DataType.Text)]
        [StringLength(50)]
        [RegularExpression(@"[a-zA-Z]{2,}$")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [DataType(DataType.Text)]
        [StringLength(50)]
        [RegularExpression(@"[a-zA-Z]{2,}$")]
        public string LastName { get; set; }


        [Required]
        [Display(Name = "Orders History")]
        public List<Order> OrdersHistory { get; set; }


        public Address? Address { get; set; }

    }
}