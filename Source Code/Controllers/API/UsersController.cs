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
        /// Logs in User and returns a JWT
        /// </summary>
        [AllowAnonymous]
        [HttpPost("Login")]
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
                    new Claim(ClaimTypes.Name, user.Id),
                    new Claim(ClaimTypes.NameIdentifier, user.UserName),
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
        /// Get every User registered.
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
            var dtos = users
                .Select(u => UserDto.ConvertBack(u, userWithRoles.ContainsKey(u.Id) ? userWithRoles[u.Id] : null))
                .ToArray();

            //Order by RoleId and then by Username
            dtos = dtos
                .Where(u => u.Role != null && (isAdmin || u.Role.Id != AdministratorRoleId)) //If not an Administrator, filter non-Administrator Users
                .OrderBy(u => u.Role.Id)
                .ThenBy(u => u.UserName)
                .ToArray();

            return Ok(dtos);
        }
        #endregion

        #region Get
        /// <summary>
        /// Gets a single User by its Username
        /// </summary>
        /// <param name="username">The Username of the user to be found</param>
        [HttpGet("{username}")]
        public async Task<ActionResult> Get(string username)
        {
            var userInDb = await userManager.FindByNameAsync(username);

            if (userInDb == null)
                return NotFound();

            string roleName = await GetUserRoleName(userInDb);
            if (User.IsInRole(UserManagerRoleName) && roleName == AdministratorRoleName)
                return Forbid();

            return Ok(UserDto.ConvertBack(userInDb, RoleHelper.GetRoleId(roleName)));
        }
        #endregion

        #region GetByRole
        /// <summary>
        /// Get every User registered by its Role
        /// </summary>
        [HttpGet("GetByRole/{roleId}")]
        public async Task<ActionResult> GetByRole(string roleId)
        {
            //If User is not an Admin, and passed the Admin RoleId, return Forbidden
            if (!User.IsInRole(AdministratorRoleName) && roleId == AdministratorRoleId)
                return Forbid();

            //Check if the role is valid before going on
            if(!ValidateRole(roleId))
                return BadRequest("The provided roleId is not valid");

            //Get users in DB and filter by roleId
            var users = await userManager.GetUsersInRoleAsync(RoleHelper.GetRoleName(roleId));

            //Order by Username, then convert to Dto
            var dtos = users
                .Select(u => UserDto.ConvertBack(u, roleId))
                .OrderBy(u => u.UserName)
                .ToArray();

            return Ok(dtos);
        }
        #endregion

        #region Post
        /// <summary>
        /// Create a new User.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Post(UserDto dto)
        {
            //Validate RoleId
            if (!User.IsInRole(AdministratorRoleName) && dto.RoleId == AdministratorRoleId)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("The Password is required.");

            if (!ValidateRole(dto.RoleId))
                return BadRequest("The provided roleId is not valid");
            
            try
            {
                dto.Id = await CreateUser(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            //Everything went fine, prepare the Dto and make sure NOT to return the password
            dto.Password = null;
            dto.Role = new RoleDto(dto.RoleId);
            dto.RoleId = null;

            //Return location of the new User along with the Dto
            return Created( Url.Action("Get", "Users", new { dto.UserName }), dto);
        }
        #endregion

        #region Put
        /// <summary>
        /// Updates a specific User.
        /// </summary>
        [HttpPut("{username}")]
        public async Task<ActionResult> Put(string username, [FromBody]UserDto dto)
        {
            //Validate RoleId
            if (!User.IsInRole(AdministratorRoleName) && dto.RoleId == AdministratorRoleId)
                return Forbid();

            //We should not allow a User to update itself
            if (User.FindFirst(ClaimTypes.NameIdentifier).Value.ToUpper() == username.ToUpper())
                return BadRequest("Sorry, but you can't edit yourself. If you want to change your Username and/or Password, check the Manage API.");

            if (dto.RoleId != null && !ValidateRole(dto.RoleId))
                return BadRequest("The provided roleId is not valid");

            //Get User in the DB
            var userInDb = await userManager.FindByNameAsync(username);

            //Return NotFound if null
            if (userInDb == null) 
                return NotFound();

            //Change username only if it was supplied
            if (!string.IsNullOrWhiteSpace(dto.UserName))
                userInDb.UserName = dto.UserName;

            //Change password only if it was supplied
            if (dto.Password != null)
            {
                //Validates password
                var passwordResult = await userManager.PasswordValidators.First().ValidateAsync(userManager, userInDb, dto.Password);

                //Return BadRequest if a validation error was found
                if (!passwordResult.Succeeded)
                    return BadRequest(string.Join(Environment.NewLine, passwordResult.Errors.Select(e => e.Description)));

                //Set password if no error were found
                userInDb.PasswordHash = userManager.PasswordHasher.HashPassword(userInDb, dto.Password);
            }

            //Attemp to update the user
            var result = await userManager.UpdateAsync(userInDb);

            //If any error was found, return it as a BadRequest
            if (!result.Succeeded)
                return BadRequest(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));

            //Change the RoleId if they are different
            string role = RoleHelper.GetRoleName(dto.RoleId);
            if (dto.RoleId != null && !(await userManager.IsInRoleAsync(userInDb, role)))
            {
                string currentRole = await GetUserRoleName(userInDb);
                await userManager.RemoveFromRoleAsync(userInDb, currentRole);
                await userManager.AddToRoleAsync(userInDb, role);
            }

            return Ok();
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes a specific User.
        /// </summary>
        [HttpDelete("{username}")]
        public async Task<ActionResult> Delete(string username)
        {
            //Avoid a User to delete itself
            if (User.FindFirst(ClaimTypes.NameIdentifier).Value.ToUpper() == username.ToUpper())
                return BadRequest("Sorry, but you can't delete yourself!");

            //Get the User
            var user = await userManager.FindByNameAsync(username);

            //Return NotFound if null
            if (user == null)
                return NotFound();

            //Prevents a non-Administrator from deleting an Administrator
            if (!User.IsInRole(AdministratorRoleName) && await GetUserRoleName(user) == AdministratorRoleName)
                return Forbid();

            //Removes the User
            await userManager.DeleteAsync(user);

            return NoContent();
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
                throw new ArgumentException(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));

            //Create the UserRole entry
            result = await userManager.AddToRoleAsync(user, RoleHelper.GetRoleName(dto.RoleId));

            //If the role was not set, throw exception
            if (!result.Succeeded)
                throw new ArgumentException(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));

            //Return the user id
            return user.Id;
        }

        /// <summary>
        /// Validates if the provided RoleId is valid
        /// </summary>
        private bool ValidateRole(string roleId)
        {
            return RoleHelper.GetRoleName(roleId) != null;
        }

        /// <summary>
        /// Gets the Role Name from a given User
        /// </summary>
        /// <param name="user">The User to get the Role from</param>
        /// <returns>The Role Name of the User</returns>
        private async Task<string> GetUserRoleName(IdentityUser user)
        {
            return (await userManager.GetRolesAsync(user)).SingleOrDefault();
        }
        #endregion
    }
}