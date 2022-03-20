using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrackr.Data;
using MoneyTrackr.Shared.DTOs;

namespace MoneyTrackr.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly MoneyTrackrDbContext dbContext;

        #region Constructor
        public RolesController(MoneyTrackrDbContext _dbContext)
        {
            dbContext = _dbContext;
        }
        #endregion

        [HttpGet]
        public async Task<IEnumerable<RoleDto>> Get()
        {
            // Retrieves all Roles and convert into DTOs
            var roles = await dbContext.Roles
               .Select(r => new RoleDto(r.Id, r.Name))
               .ToArrayAsync();

            // Retrieves all RoleClaims
            var roleClaims = await dbContext.RoleClaims.ToListAsync();

            // Group RoleClaims based on each Role and convert into DTOs
            var groupedRoleClaims = roleClaims
                .GroupBy(rc => rc.RoleId)
                .ToDictionary(
                    rc => rc.Key,
                    rc => rc
                        .Select(c => new RoleClaimDto(c.ClaimType, c.ClaimValue))
                        .ToArray()
                    );

            // Assign Claims to each Role
            foreach (var r in roles)
                r.Claims = groupedRoleClaims[r.Id];

            return roles;
        }
    }
}
