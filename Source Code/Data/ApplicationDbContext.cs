using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MoneyTrackr.Data.DomainObjects;
using static MoneyTrackr.Constants.Role;
using static MoneyTrackr.Constants.User;

namespace MoneyTrackr.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public IConfiguration Configuration { get; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            IConfiguration configuration)
            : base(options)
        {
            Configuration = configuration;
        }

        public DbSet<Entry> Entries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            string adminUserId = "1d954590-2b9e-4f2f-922c-4aeaf62c6889";
            string userManagerId = "f0e89618-1f43-40cd-8ac4-85988296266a";
            string regularUserId = "47a17c9f-be3a-44fb-a48c-70930ecd2b40";

            PasswordHasher<IdentityUser> passwordHasher = new PasswordHasher<IdentityUser>();

            modelBuilder.Entity<IdentityRole>().HasData
            (
                new IdentityRole
                {
                    Id = AdministratorRoleId,
                    Name = AdministratorRoleName,
                    NormalizedName = NormalizedAdministratorRoleName,
                    ConcurrencyStamp = "b592db73-d6a0-4ddc-b552-b0cdf96d6741"
                },
                new IdentityRole
                {
                    Id = UserManagerRoleId,
                    Name = UserManagerRoleName,
                    NormalizedName = NormalizedUserManagerRoleName,
                    ConcurrencyStamp = "7b053b9d-a08f-4eac-ba9e-067caeb1c08f"
                },
                new IdentityRole
                {
                    Id = RegularUserRoleId,
                    Name = RegularUserRoleName,
                    NormalizedName = NormalizedRegularUserRoleName,
                    ConcurrencyStamp = "8f253445-8f89-44f3-bb07-57c3ecccdaa7"
                }
            );

            var adminUser = new IdentityUser
            {
                Id = adminUserId,
                UserName = AdminUserName,
                NormalizedUserName = AdminUserName.ToUpper(),
                LockoutEnabled = true,
                SecurityStamp = "2016e79f-f4ac-42af-a277-406f6ffabe56",
                ConcurrencyStamp = "55270b70-0750-4fd9-8fed-3a46bcd06185"
            };
            var managerUser = new IdentityUser
            {
                Id = userManagerId,
                UserName = ManagerUserName,
                NormalizedUserName = ManagerUserName.ToUpper(),
                LockoutEnabled = true,
                SecurityStamp = "88dc18da-0d7a-4d8f-85d1-81a08e23efa3",
                ConcurrencyStamp = "4c566e14-886a-46ff-9ee7-f4ef0c5bbb11"
            };
            var regularUser = new IdentityUser
            {
                Id = regularUserId,
                UserName = RegularUserName,
                NormalizedUserName = RegularUserName.ToUpper(),
                LockoutEnabled = true,
                SecurityStamp = "7ff4e046-cc54-44ea-9e1a-067e02694d07",
                ConcurrencyStamp = "c8c3bb98-aaa3-4216-895f-55c0a61250ce"
            };

            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, Configuration["Passwords:AdminPassword"]);
            managerUser.PasswordHash = passwordHasher.HashPassword(managerUser, Configuration["Passwords:ManagerPassword"]);
            regularUser.PasswordHash = passwordHasher.HashPassword(regularUser, Configuration["Passwords:RegularPassword"]);
            modelBuilder.Entity<IdentityUser>().HasData(adminUser, managerUser, regularUser);
            
            modelBuilder.Entity<IdentityUserRole<string>>().HasData
            (
                new IdentityUserRole<string>
                {
                    UserId = adminUserId,
                    RoleId = AdministratorRoleId
                },
                new IdentityUserRole<string>
                {
                    UserId = userManagerId,
                    RoleId = UserManagerRoleId
                },
                new IdentityUserRole<string>
                {
                    UserId = regularUserId,
                    RoleId = RegularUserRoleId
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
