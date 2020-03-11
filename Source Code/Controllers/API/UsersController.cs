using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoneyTrackr.Data;
using MoneyTrackr.Dtos;
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

        public UsersController(ApplicationDbContext _dbContext, IConfiguration _configuration)
        {
            dbContext = _dbContext;
            configuration = _configuration;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult LogIn([FromBody] UserLogInDto userDto)
        {
            var passwordHasher = new PasswordHasher<IdentityUser>();

            var user = dbContext.Users
                .SingleOrDefault(x => x.UserName.ToLower() == userDto.Username.ToLower());

            if (user == null)
                return BadRequest("Username or Password is incorrect."); //Return BadRequest so the end user does not know this username does not exists

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userDto.Password) != PasswordVerificationResult.Success)
                return BadRequest("Username or Password is incorrect.");

            //Successful Authentication, proceed to generate token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(tokenHandler.WriteToken(token));
        }
    }
}