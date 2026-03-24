using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Yummiez.Data
{
    public class YummiezDbContextFactory : IDesignTimeDbContextFactory<YummiezDbContext>
    {
        public YummiezDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
            {
                basePath = AppContext.BaseDirectory ?? basePath;
                var dir = new DirectoryInfo(basePath);
                while (dir != null && !File.Exists(Path.Combine(dir.FullName, "appsettings.json")))
                {
                    dir = dir.Parent;
                }

                if (dir != null)
                {
                    basePath = dir.FullName;
                }
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("ApplicationDbContext")
                ?? throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<YummiezDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new YummiezDbContext(optionsBuilder.Options);
        }
    }
}
