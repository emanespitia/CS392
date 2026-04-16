using Yummiez.Constants;
using Yummiez.Data;
using Yummiez.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Yummiez.Data
{
    public class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            var userManager = service.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

            var roles = Enum.GetNames<Roles>();
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            //create admin user
            var user = new IdentityUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };

            var userInDb = await userManager.FindByEmailAsync(user.Email);
            if (userInDb == null)
            {
               await userManager.CreateAsync(user, "Admin@123");
               userInDb = user;
            }

            if (userInDb != null && !await userManager.IsInRoleAsync(userInDb, Roles.Admin.ToString()))
            {
                await userManager.AddToRoleAsync(userInDb, Roles.Admin.ToString());
            }
        }

        public static async Task SeedRestaurantsAsync(YummiezDbContext context)
        {
            try
            {
                // Add new columns if they don't exist yet
                var addColumnsSql = @"
                    IF COL_LENGTH('Restaurants', 'category') IS NULL
                        ALTER TABLE Restaurants ADD category NVARCHAR(50);
                    IF COL_LENGTH('Restaurants', 'image_url') IS NULL
                        ALTER TABLE Restaurants ADD image_url NVARCHAR(500);
                    IF COL_LENGTH('Restaurants', 'manager_user_id') IS NULL
                        ALTER TABLE Restaurants ADD manager_user_id NVARCHAR(450) NULL;
                    IF COL_LENGTH('Orders', 'driver_user_id') IS NULL
                        ALTER TABLE Orders ADD driver_user_id NVARCHAR(450) NULL;
                ";
                await context.Database.ExecuteSqlRawAsync(addColumnsSql);

                if (await context.Restaurants.AnyAsync())
                {
                    // Update existing rows that are missing category/image
                    var updateSql = @"
                        UPDATE Restaurants SET category = 'Burgers', image_url = 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400' WHERE name = 'Burger Joint' AND category IS NULL;
                        UPDATE Restaurants SET category = 'Pizza', image_url = 'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400' WHERE name = 'Mama''s Pizza' AND category IS NULL;
                        UPDATE Restaurants SET category = 'Sushi', image_url = 'https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=400' WHERE name = 'Sakura Sushi' AND category IS NULL;
                        UPDATE Restaurants SET category = 'Healthy', image_url = 'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400' WHERE name = 'Green Bowl' AND category IS NULL;
                        UPDATE Restaurants SET category = 'Mexican', image_url = 'https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=400' WHERE name = 'Taco Fiesta' AND category IS NULL;
                    ";
                    await context.Database.ExecuteSqlRawAsync(updateSql);
                    return;
                }

                // Fresh seed with all data
                var sql = @"
                    ALTER TABLE Restaurants NOCHECK CONSTRAINT ALL;

                    INSERT INTO Restaurants (name, owner_name, address, phone, is_open, admin_id, category, image_url, created_at)
                    VALUES
                        ('Burger Joint', 'John Smith', '123 Main St, Newark, NJ', '(973) 555-0101', 1, 0, 'Burgers', 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400', GETUTCDATE()),
                        ('Mama''s Pizza', 'Maria Rossi', '456 Broad St, Newark, NJ', '(973) 555-0202', 1, 0, 'Pizza', 'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400', GETUTCDATE()),
                        ('Sakura Sushi', 'Yuki Tanaka', '789 Market St, Newark, NJ', '(973) 555-0303', 1, 0, 'Sushi', 'https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=400', GETUTCDATE()),
                        ('Green Bowl', 'Sarah Johnson', '321 Park Ave, Newark, NJ', '(973) 555-0404', 1, 0, 'Healthy', 'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400', GETUTCDATE()),
                        ('Taco Fiesta', 'Carlos Garcia', '654 University Ave, Newark, NJ', '(973) 555-0505', 0, 0, 'Mexican', 'https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=400', GETUTCDATE());

                    ALTER TABLE Restaurants CHECK CONSTRAINT ALL;
                ";
                await context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception)
            {
                // Seeding may fail — app still starts
            }
        }
    }
}

