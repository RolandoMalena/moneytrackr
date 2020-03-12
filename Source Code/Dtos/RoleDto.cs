using Microsoft.AspNetCore.Identity;
using MoneyTrackr.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyTrackr.Dtos
{
    public class RoleDto
    {
        public RoleDto(string roleId)
        {
            Id = roleId;
            Name = RoleHelper.GetRoleName(roleId);
        }

        public string Id { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Converts a Model Object into a new instance of a Dto, optionally with the RoleId
        /// </summary>
        public static RoleDto ConvertBack(IdentityRole role)
        {
            return new RoleDto(role.Id);
        }
    }
}
