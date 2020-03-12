using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackr.Data;
using MoneyTrackr.Dtos;

namespace MoneyTrackr.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;

        #region Constructor
        public ManageController(UserManager<IdentityUser> _userManager)
        {
            userManager = _userManager;
        }
        #endregion

        #region ChangePassword
        [HttpPatch("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            //Check that the passwords aren't equal
            if (dto.CurrentPassword == dto.NewPassword)
                return BadRequest("The New Password cannot be equal to the Current Password.");

            var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.Name).Value);

            //Verify if the CurrentPassword is correct
            var passwordHasher = new PasswordHasher<IdentityUser>();
            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword) != PasswordVerificationResult.Success)
                return BadRequest("The Current Password is incorrect.");

            //Procceed to change the password
            await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            return Ok();
        }
        #endregion
    }
}