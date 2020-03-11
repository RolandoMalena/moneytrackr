using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using static MoneyTrackr.Constants.Role;

namespace MoneyTrackr.Data
{
    public interface IApplicationDbContext
    {
        DbSet<IdentityRole> Roles { get; set; }
        DbSet<IdentityUser> Users { get; set; }
        public DbSet<IdentityUserRole<string>> UserRoles { get; set; }
        //DbSet<Entry> Entries { get; set; }

        Task<int> SaveChangesAsync();
    }

    public class ApplicationDbContext : IdentityDbContext, IApplicationDbContext
    {
        public IConfiguration Configuration { get; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            IConfiguration configuration)
            : base(options)
        {
            Configuration = configuration;
        }

        public Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            string adminUserId = Guid.NewGuid().ToString();
            string userManagerId = Guid.NewGuid().ToString();
            string regularUserId = Guid.NewGuid().ToString();

            PasswordHasher<IdentityUser> passwordHasher = new PasswordHasher<IdentityUser>();

            modelBuilder.Entity<IdentityRole>().HasData
            (
                new IdentityRole
                {
                    Id = AdministratorRoleId,
                    Name = AdministratorRoleName,
                    NormalizedName = AdministratorRoleName
                },
                new IdentityRole
                {
                    Id = UserManagerRoleId,
                    Name = UserManagerRoleName,
                    NormalizedName = UserManagerRoleName
                },
                new IdentityRole
                {
                    Id = RegularUserRoleId,
                    Name = RegularUserRoleName,
                    NormalizedName = RegularUserRoleName
                }
            );

            var adminUser = new IdentityUser
            {
                Id = adminUserId,
                UserName = "Admin",
                NormalizedUserName = "Admin"
            };
            var managerUser = new IdentityUser
            {
                Id = userManagerId,
                UserName = "Manager",
                NormalizedUserName = "Manager"
            };
            var regularUser = new IdentityUser
            {
                Id = regularUserId,
                UserName = "Regular",
                NormalizedUserName = "Regular"
            };

            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, Configuration["AdminPassword"]);
            managerUser.PasswordHash = passwordHasher.HashPassword(managerUser, "manager1");
            regularUser.PasswordHash = passwordHasher.HashPassword(managerUser, "regular1");
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
