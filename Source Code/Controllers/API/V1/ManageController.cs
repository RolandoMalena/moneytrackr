﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackr.Dtos;

namespace MoneyTrackr.Controllers.API.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ManageController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;

        #region Constructor
        public ManageController(UserManager<IdentityUser> _userManager)
        {
            userManager = _userManager;
        }
        #endregion

        #region ChangeUserName
        /// <summary>
        /// Changes your Username
        /// </summary>
        /// <param name="dto">The new Username along with the Password for verification.</param>
        [HttpPatch("ChangeUsername")]
        public async Task<IActionResult> ChangeUserName([FromBody] ChangeUserNameDto dto)
        {
            var user = await userManager.FindByNameAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            //Verify if the CurrentPassword is correct
            var passwordHasher = new PasswordHasher<IdentityUser>();
            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword) != PasswordVerificationResult.Success)
                return BadRequest("The Current Password is incorrect.");

            //Procceed to change the username
            user.UserName = dto.UserName;
            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));

            return Ok();
        }
        #endregion

        #region ChangePassword
        /// <summary>
        /// Changes your Password
        /// </summary>
        /// <param name="dto">The new Password along with the current one for verification.</param>
        [HttpPatch("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var user = await userManager.FindByNameAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            //Verify if the CurrentPassword is correct
            var passwordHasher = new PasswordHasher<IdentityUser>();
            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword) != PasswordVerificationResult.Success)
                return BadRequest("The Current Password is incorrect.");

            //Procceed to change the password
            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));

            return Ok();
        }
        #endregion
    }
}