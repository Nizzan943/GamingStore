using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GamingStore.Contracts;
using GamingStore.Data;
using GamingStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GamingStore.Areas.Identity.Pages.Account.Manage
{
    public class SetAddressModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public SetAddressModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string? FullName { get; set; }
        public string? Address1 { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; } = "Israel";

        public string StatusMessage { get; set; }
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            [DataType(DataType.Text)]
            public string? FullName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Address")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Address is not valid")]
            public string? Address1 { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "*City is not valid")]
            public string? City { get; set; }

            [Required]
            [DataType(DataType.PostalCode)]
            [Display(Name = "Postal Code")]
            [Range(00000, 9999999, ErrorMessage = "Postal code is not valid")]
            public string? PostalCode { get; set; }

            [DataType(DataType.Text)]
            public string? Country { get; set; } = "Israel";
        }

        private void Load(User user)
        {
            try
            {
                FullName = $"{user.FirstName} {user.LastName}";
                Address1 = user.Address.Address1;
                City = user.Address.City;
                PostalCode = user.Address.PostalCode;
                Country = user.Address.Country;
            }
            catch
            {
                FullName = $"{user.FirstName} {user.LastName}";
                Address1 = null;
                City = null;
                PostalCode = null;
                Country = null;
            }

            Input = new InputModel
            {
                FullName = FullName,
                Address1 =  Address1,
                City = City,
                Country = Country,
                PostalCode = PostalCode
                
            };
        }

        public async Task<IActionResult> OnPostSaveAddressAsync(Address address)
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            user.Address = address;
            await _userManager.UpdateAsync(user);

            StatusMessage = "Your address is unchanged.";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnGetAsync()
        {
            User user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Load(user);
            return Page();
        }
    }
}