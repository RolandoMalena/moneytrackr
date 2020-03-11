using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoneyTrackr.Data;
using MoneyTrackr.Dtos;
using MoneyTrackr.Helpers;
using static MoneyTrackr.Constants.Role;

namespace MoneyTrackr.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AdministratorRoleName + "," + UserManagerRoleName)]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration configuration;
        private readonly UserManager<IdentityUser> userManager;


        #region Constructor
        public UsersController(
            ApplicationDbContext _dbContext, 
            IConfiguration _configuration,
            UserManager<IdentityUser> _userManager)
        {
            dbContext = _dbContext;
            configuration = _configuration;
            userManager = _userManager;
        }
        #endregion

        #region LogIn
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] UserLogInDto userDto)
        {
            var passwordHasher = new PasswordHasher<IdentityUser>();
            
            var user = dbContext.Users
                .SingleOrDefault(x => x.UserName.ToLower() == userDto.Username.ToLower());

            if (user == null)
                return BadRequest("Username or Password is incorrect."); //Return BadRequest so the end user does not know this username does not exists

            if (user.LockoutEnd.HasValue && DateTime.Now <= user.LockoutEnd.Value)
                return BadRequest("User has been blocked out, try again after 5 minutes have passed.");

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userDto.Password) != PasswordVerificationResult.Success)
            {
                await userManager.AccessFailedAsync(user);

                if (user.AccessFailedCount <= 2)
                    return BadRequest("Username or Password is incorrect.");
                else
                    return BadRequest("User has been blocked out, try again after 5 minutes have passed.");
            }

            //Successful Authentication, proceed to generate token
            string roleId = dbContext.UserRoles.SingleOrDefault(u => u.UserId == user.Id).RoleId;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, RoleHelper.GetRoleName(roleId))
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var response = new
            {
                auth_token = tokenHandler.WriteToken(token)
            };

            await userManager.ResetAccessFailedCountAsync(user);
            return Ok(response);
        }
        #endregion
    }
}