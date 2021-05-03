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

        public Address Address { get; set; }
        public string StatusMessage { get; set; }
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public Address Address { get; set; }
        }

        private void Load(User user)
        {
            Address = user.Address;


            Input = new InputModel
            {
                Address = Address
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