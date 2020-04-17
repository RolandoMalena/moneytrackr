using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MoneyTrackr.Areas.Identity.Pages.Account.Manage
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public DeletePersonalDataModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public IActionResult OnGet()
        {
            return NotFound();

            //var user = await _userManager.GetUserAsync(User);
            //if (user == null)
            //{
            //    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            //}

            //RequirePassword = await _userManager.HasPasswordAsync(user);
            //return Page();
        }

        public IActionResult OnPostAsync()
        {
            return NotFound();

            //var user = await _userManager.GetUserAsync(User);
            //if (user == null)
            //{
            //    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            //}

            //RequirePassword = await _userManager.HasPasswordAsync(user);
            //if (RequirePassword)
            //{
            //    if (!await _userManager.CheckPasswordAsync(user, Input.Password))
            //    {
            //        ModelState.AddModelError(string.Empty, "Incorrect password.");
            //        return Page();
            //    }
            //}

            //var result = await _userManager.DeleteAsync(user);
            //var userId = await _userManager.GetUserIdAsync(user);
            //if (!result.Succeeded)
            //{
            //    throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
            //}

            //await _signInManager.SignOutAsync();

            //_logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            //return Redirect("~/");
        }
    }
}
