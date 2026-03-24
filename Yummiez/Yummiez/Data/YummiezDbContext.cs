using Microsoft.EntityFrameworkCore;
using Yummiez.Models;

namespace Yummiez.Data
{
    public class YummiezDbContext : DbContext
    {
        public YummiezDbContext(DbContextOptions<YummiezDbContext> options) : base(options)
        {
        }

        public DbSet<Restaurant> Restaurants { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Driver> Drivers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // If the Restaurants table has database triggers, tell EF Core to not use
            // the SQL Server OUTPUT clause for this table so SaveChanges will work.
            // This reverts to the older (slower) behavior that is compatible with triggers.
            modelBuilder.Entity<Restaurant>()
                .ToTable(tb => tb.UseSqlOutputClause(false));

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.IdentityUserId)
                .HasFilter("[identity_user_id] IS NOT NULL")
                .IsUnique();

            modelBuilder.Entity<Driver>()
                .HasIndex(d => d.IdentityUserId)
                .HasFilter("[identity_user_id] IS NOT NULL")
                .IsUnique();
        }
    }
}