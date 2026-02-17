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
    }
}