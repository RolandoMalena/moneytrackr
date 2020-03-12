using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrackr.Data;
using MoneyTrackr.Dtos;
using static MoneyTrackr.Constants.Role;

namespace MoneyTrackr.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AdministratorRoleName + "," + UserManagerRoleName)]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        #region Constructor
        public RolesController(ApplicationDbContext _dbContext)
        {
            dbContext = _dbContext;
        }
        #endregion

        #region Get
        /// <summary>
        /// Get all Roles
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            bool isAdmin = User.IsInRole(AdministratorRoleName);

            var roles = await dbContext.Roles
                .Where(r => isAdmin || r.Id != AdministratorRoleId)
                .ToListAsync();

            return Ok(roles.Select(r => RoleDto.ConvertBack(r)).ToArray());
        }
        #endregion
    }
}