using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackr.Data
{
    public class MoneyTrackrDbContext : IdentityDbContext
    {
        public MoneyTrackrDbContext()
        {
        }

        public MoneyTrackrDbContext(DbContextOptions<MoneyTrackrDbContext> opts) : base(opts)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
            base.OnModelCreating(modelBuilder);
        }
    }
}
