using Microsoft.EntityFrameworkCore.Migrations;
using MoneyTrackr.Data.Migrations.Helpers;
using static MoneyTrackr.Shared.Constants;

#nullable disable

namespace MoneyTrackr.Api.Migrations
{
    public partial class AddRolesClaimsAndInitialUser : Migration
    {
        private const string RolesTableName = @"AspNetRoles";
        private readonly string[] RolesColumNames =
        {
            "Id", "Name", "NormalizedName", "ConcurrencyStamp"
        };

        private const string UsersTableName = "AspNetUsers";
        private readonly string[] UsersColumNames =
        {
            "Id", "Email", "SecurityStamp", "ConcurrencyStamp", "EmailConfirmed", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
        };

        private const string UserRolesTableName = "AspNetUserRoles";
        private readonly string[] UserRolesColumNames =
        {
            "UserId", "RoleId"
        };

        private const string RoleClaimsTableName = "AspNetRoleClaims";
        private readonly string[] RoleClaimsColumNames =
        {
            "RoleId", "ClaimType", "ClaimValue"
        };

        private record Role(string Id, string Name, string ConcurrencyStamp);
        private readonly Role AdministratorRole = new("1", "Administrator", "b592db73-d6a0-4ddc-b552-b0cdf96d6741");
        private readonly Role UserManagerRole = new("2", "User Manager", "7b053b9d-a08f-4eac-ba9e-067caeb1c08f");
        private readonly Role RegularUserRole = new("3", "Regular User", "8f253445-8f89-44f3-bb07-57c3ecccdaa7");

        private record User(string Id, string Email, string SecurityStamp, string ConcurrencyStamp, bool EmailConfirmed, bool PhoneNumberConfirmed, bool TwoFactorEnabled, bool LockoutEnabled, int AccessFailedCount);
        private readonly User AdministratorUser = new("1d954590-2b9e-4f2f-922c-4aeaf62c6889", "admin@moneytrackr.com", "2016e79f-f4ac-42af-a277-406f6ffabe56", "55270b70-0750-4fd9-8fed-3a46bcd06185", true, false, false, false, 0);

        private record RoleClaim(string ClaimType, string ClaimValue);
        private readonly RoleClaim[] AdministratorRoleClaims = new RoleClaim[]
        {
            new(ClaimTypes.FinanceManagementLevel, FinanceManagementLevels.Self),
            new(ClaimTypes.FinanceManagementLevel, FinanceManagementLevels.RegularUsers),
            new(ClaimTypes.FinanceManagementLevel, FinanceManagementLevels.UserManagers),
            new(ClaimTypes.UserManagementLevel, UserManagementLevels.RegularUsers),
            new(ClaimTypes.UserManagementLevel, UserManagementLevels.UserManagers),
        };
        private readonly RoleClaim[] UserManagerRoleClaims = new RoleClaim[]
        {
            new(ClaimTypes.FinanceManagementLevel, FinanceManagementLevels.Self),
            new(ClaimTypes.FinanceManagementLevel, FinanceManagementLevels.RegularUsers),
            new(ClaimTypes.UserManagementLevel, UserManagementLevels.RegularUsers)
        };
        private readonly RoleClaim[] RegularUserRoleClaims = new RoleClaim[]
        {
            new(ClaimTypes.FinanceManagementLevel, FinanceManagementLevels.Self),
            new(ClaimTypes.UserManagementLevel, UserManagementLevels.None)
        };

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Insert(RolesTableName, RolesColumNames, new object[][]
            {
                new object[] { AdministratorRole.Id, AdministratorRole.Name, AdministratorRole.Name.ToUpper(), AdministratorRole.ConcurrencyStamp },
                new object[] { UserManagerRole.Id, UserManagerRole.Name, UserManagerRole.Name.ToUpper(), AdministratorRole.ConcurrencyStamp },
                new object[] { RegularUserRole.Id, RegularUserRole.Name, RegularUserRole.Name.ToUpper(), AdministratorRole.ConcurrencyStamp },
            });

            migrationBuilder.Insert(UsersTableName, UsersColumNames, new object[][]
            {
                new object[] { AdministratorUser.Id, AdministratorUser.Email, AdministratorUser.SecurityStamp, AdministratorUser.ConcurrencyStamp, AdministratorUser.EmailConfirmed, AdministratorUser.PhoneNumberConfirmed, AdministratorUser.TwoFactorEnabled, AdministratorUser.LockoutEnabled, AdministratorUser.AccessFailedCount },
            });

            migrationBuilder.Insert(UserRolesTableName, UserRolesColumNames, new object[][]
            {
                new object[] { AdministratorUser.Id, AdministratorRole.Id },
            });

            migrationBuilder.Insert(RoleClaimsTableName, RoleClaimsColumNames, new object[][]
            {
                new object[] { AdministratorRole.Id, AdministratorRoleClaims[0].ClaimType, AdministratorRoleClaims[0].ClaimValue },
                new object[] { AdministratorRole.Id, AdministratorRoleClaims[1].ClaimType, AdministratorRoleClaims[1].ClaimValue },
                new object[] { AdministratorRole.Id, AdministratorRoleClaims[2].ClaimType, AdministratorRoleClaims[2].ClaimValue },
                new object[] { AdministratorRole.Id, AdministratorRoleClaims[3].ClaimType, AdministratorRoleClaims[3].ClaimValue },
                new object[] { AdministratorRole.Id, AdministratorRoleClaims[4].ClaimType, AdministratorRoleClaims[4].ClaimValue },
                
                new object[] { UserManagerRole.Id, UserManagerRoleClaims[0].ClaimType, UserManagerRoleClaims[0].ClaimValue },
                new object[] { UserManagerRole.Id, UserManagerRoleClaims[1].ClaimType, UserManagerRoleClaims[1].ClaimValue },
                new object[] { UserManagerRole.Id, UserManagerRoleClaims[2].ClaimType, UserManagerRoleClaims[2].ClaimValue },
                
                new object[] { RegularUserRole.Id, RegularUserRoleClaims[0].ClaimType, RegularUserRoleClaims[0].ClaimValue },
                new object[] { RegularUserRole.Id, RegularUserRoleClaims[1].ClaimType, RegularUserRoleClaims[1].ClaimValue },
            });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DELETE FROM {RoleClaimsTableName};");
            migrationBuilder.Sql($"DELETE FROM {UserRolesTableName};");
            migrationBuilder.Sql($"DELETE FROM {UsersTableName};");
            migrationBuilder.Sql($"DELETE FROM {RolesTableName};");
        }
    }
}
