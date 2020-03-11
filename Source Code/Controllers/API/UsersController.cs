using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        /// <summary>
        /// Log in User and returns a JWT if succeeds
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] UserLogInDto userDto)
        {
            var passwordHasher = new PasswordHasher<IdentityUser>();
            
            var user = await dbContext.Users
                .SingleOrDefaultAsync(x => x.UserName.ToLower() == userDto.Username.ToLower());

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
            string roleId = (await dbContext.UserRoles
                .SingleOrDefaultAsync(u => u.UserId == user.Id)).RoleId;

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

        #region Register
        /// <summary>
        /// Register a new User. Will be given a Regular User role.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult> Register(RegisterUserDto dto)
        {
            try
            {
                //Attempts to create User and assign role
                await CreateUser(dto);
            }
            catch (ArgumentException ex)
            {
                //An error was found, return as BadRequest
                return BadRequest(ex.Message);
            }

            //Everything went fine to this point
            return Ok();
        }
        #endregion

        #region GetAll
        /// <summary>
        /// Get every User registered in the Database. If the Logged In user is an User Manager, it won't return Administrators
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            bool isAdmin = User.IsInRole(AdministratorRoleName);

            //Start by getting a list of User IDs with Role IDs
            var userWithRoles = await dbContext.UserRoles
                .ToDictionaryAsync
                (
                    ur => ur.UserId,
                    ur => ur.RoleId
                );

            //Initialize query by getting all users
            var users = await dbContext.Users.ToListAsync();

            //Convert Models into Dtos
            var dtos = users.Select(u => UserDto.ConvertBack(u, userWithRoles[u.Id])).ToArray();

            //Order by RoleId and then by Username
            dtos = dtos
                .Where(u => isAdmin || u.RoleId != AdministratorRoleId) //If not an Administrator, filter non-Administrator Users
                .OrderBy(u => u.RoleId)
                .ThenBy(u => u.UserName)
                .ToArray();

            return Ok(dtos);
        }
        #endregion

        #region Get
        /// <summary>
        /// Gets a single User by its Id
        /// </summary>
        /// <param name="id">The Id of the user to be found</param>
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(string id)
        {
            var userInDb = await userManager.FindByIdAsync(id.ToString());

            if (userInDb == null)
                return NotFound();

            string roleName = (await userManager.GetRolesAsync(userInDb)).Single();
            if (User.IsInRole(UserManagerRoleName) && roleName == AdministratorRoleName)
                return Forbid();

            return Ok(UserDto.ConvertBack(userInDb, RoleHelper.GetRoleId(roleName)));
        }
        #endregion

        #region GetByRole
        /// <summary>
        /// Get every User registered by its RoleId
        /// </summary>
        [HttpGet("GetByRole/{roleId}")]
        public async Task<ActionResult> GetByRole(string roleId)
        {
            //If User is not an Admin, and passed the Admin RoleId, return Forbidden
            if (!User.IsInRole(AdministratorRoleName) && roleId == AdministratorRoleId)
                return Forbid();

            //Check if the role is valid before going on
            var role = await dbContext.Roles.SingleOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
                return BadRequest("The provided roleId is not valid");

            //Get users in DB and filter by roleId
            var users = await userManager.GetUsersInRoleAsync(role.Name);

            //Order by Username, then convert to Dto
            var dtos = users
                .Select(u => UserDto.ConvertBack(u))
                .OrderBy(u => u.UserName)
                .ToArray();

            return Ok(dtos);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Creates the user based on the Dto given and also assign Role,
        /// check for exceptions for errors.
        /// </summary>
        /// <param name="dto">User data</param>
        /// <returns>Id of the new User.</returns>
        async Task<string> CreateUser(UserDto dto)
        {
            //Create model
            var user = new IdentityUser()
            {
                UserName = dto.UserName
            };

            //This will validate the User and then create it
            var result = await userManager.CreateAsync(user, dto.Password);

            //If the user was not created, throw exception
            if (!result.Succeeded)
                throw new ArgumentException(result.Errors.First().Description);

            //Create the UserRole entry
            result = await userManager.AddToRoleAsync(user, RoleHelper.GetRoleName(dto.RoleId));

            //If the role was not set, throw exception
            if (!result.Succeeded)
                throw new ArgumentException(result.Errors.First().Description);

            //Return the user id
            return user.Id;
        }
        #endregion
    }
}