using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Yummiez.Constants;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly YummiezDbContext _context;

        public IndexModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, YummiezDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public List<UserWithRole> Users { get; set; } = new();

        // Driver Applications
        public List<DriverApplication> Applications { get; set; } = new();

        // Dashboard stats
        public int TotalRestaurants { get; set; }
        public int OpenRestaurants { get; set; }
        public int ClosedRestaurants { get; set; }
        public int TotalUsers { get; set; }

        public class UserWithRole
        {
            public string Id { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string Role { get; set; } = null!;
        }

        public IReadOnlyList<string> AssignableRoles { get; } =
        [
            Roles.User.ToString(),
            Roles.Driver.ToString(),
            Roles.Admin.ToString(),
            Roles.Manager.ToString()
        ];

        public async Task OnGetAsync()
        {
            // Stats
            TotalRestaurants = await _context.Restaurants.CountAsync();
            OpenRestaurants = await _context.Restaurants.CountAsync(r => r.IsOpen == true);
            ClosedRestaurants = TotalRestaurants - OpenRestaurants;

            var allUsers = await _userManager.Users.ToListAsync();
            TotalUsers = allUsers.Count;

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserWithRole
                {
                    Id = user.Id,
                    Email = user.Email ?? "N/A",
                    Role = roles.FirstOrDefault() ?? "User"
                });
            }

            // Load driver applications
            Applications = await _context.DriverApplications
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateRoleAsync(string userId, string role)
        {
            if (!AssignableRoles.Contains(role))
            {
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin.ToString());
                if (isAdmin && role != Roles.Admin.ToString())
                {
                    var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin.ToString());
                    if (admins.Count <= 1)
                    {
                        return RedirectToPage();
                    }
                }

                foreach (var assignableRole in AssignableRoles)
                {
                    if (await _userManager.IsInRoleAsync(user, assignableRole))
                    {
                        await _userManager.RemoveFromRoleAsync(user, assignableRole);
                    }
                }

                await _userManager.AddToRoleAsync(user, role);

                TempData["SuccessMessage"] = $"User '{user.Email}' role updated to {role}.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentUserId = _userManager.GetUserId(User);
                if (user.Id == currentUserId)
                {
                    return RedirectToPage();
                }

                var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin.ToString());
                if (isAdmin)
                {
                    var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin.ToString());
                    if (admins.Count <= 1)
                    {
                        return RedirectToPage();
                    }
                }

                var uid = user.Id;
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    DECLARE @uid NVARCHAR(450) = {uid};

                    IF COL_LENGTH('dbo.Drivers', 'user_id') IS NOT NULL
                    BEGIN
                        EXEC sp_executesql
                            N'DELETE FROM dbo.Drivers WHERE user_id = @p_uid',
                            N'@p_uid NVARCHAR(450)',
                            @p_uid = @uid;
                    END;
                    IF COL_LENGTH('dbo.Drivers', 'identity_user_id') IS NOT NULL
                    BEGIN
                        EXEC sp_executesql
                            N'DELETE FROM dbo.Drivers WHERE identity_user_id = @p_uid',
                            N'@p_uid NVARCHAR(450)',
                            @p_uid = @uid;
                    END;

                    IF COL_LENGTH('dbo.Clients', 'user_id') IS NOT NULL
                    BEGIN
                        EXEC sp_executesql
                            N'DELETE FROM dbo.Clients WHERE user_id = @p_uid',
                            N'@p_uid NVARCHAR(450)',
                            @p_uid = @uid;
                    END;
                    IF COL_LENGTH('dbo.Clients', 'identity_user_id') IS NOT NULL
                    BEGIN
                        EXEC sp_executesql
                            N'DELETE FROM dbo.Clients WHERE identity_user_id = @p_uid',
                            N'@p_uid NVARCHAR(450)',
                            @p_uid = @uid;
                    END;

                    IF OBJECT_ID('dbo.Customer', 'U') IS NOT NULL AND COL_LENGTH('dbo.Customer', 'user_id') IS NOT NULL
                    BEGIN
                        EXEC sp_executesql
                            N'DELETE FROM dbo.Customer WHERE user_id = @p_uid',
                            N'@p_uid NVARCHAR(450)',
                            @p_uid = @uid;
                    END;
                    IF OBJECT_ID('dbo.Customer', 'U') IS NOT NULL AND COL_LENGTH('dbo.Customer', 'identity_user_id') IS NOT NULL
                    BEGIN
                        EXEC sp_executesql
                            N'DELETE FROM dbo.Customer WHERE identity_user_id = @p_uid',
                            N'@p_uid NVARCHAR(450)',
                            @p_uid = @uid;
                    END;

                    IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL AND COL_LENGTH('dbo.Customers', 'user_id') IS NOT NULL
                    BEGIN
                        EXEC sp_executesql
                            N'DELETE FROM dbo.Customers WHERE user_id = @p_uid',
                            N'@p_uid NVARCHAR(450)',
                            @p_uid = @uid;
                    END;
                    IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL AND COL_LENGTH('dbo.Customers', 'identity_user_id') IS NOT NULL
                    BEGIN
                        EXEC sp_executesql
                            N'DELETE FROM dbo.Customers WHERE identity_user_id = @p_uid',
                            N'@p_uid NVARCHAR(450)',
                            @p_uid = @uid;
                    END;
                ");

                await _userManager.DeleteAsync(user);

                TempData["SuccessMessage"] = $"User '{user.Email}' deleted.";
            }

            return RedirectToPage();
        }

        // APPROVE DRIVER
        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var app = await _context.DriverApplications.FindAsync(id);

            if (app != null && app.Status == "Pending")
            {
                app.Status = "Approved";

                var user = await _userManager.FindByIdAsync(app.UserId);

                if (user == null)
                    return RedirectToPage();

                if (!await _userManager.IsInRoleAsync(user, Roles.Driver.ToString()))
                {
                    await _userManager.AddToRoleAsync(user, Roles.Driver.ToString());
                }

                var exists = await _context.Drivers
                    .AnyAsync(d => d.IdentityUserId == app.UserId);

                if (!exists)
                {
                    _context.Drivers.Add(new Yummiez.Models.Driver
                    {
                        IdentityUserId = app.UserId,
                        LicenseNumber = app.LicenseNumber,
                        VehicleType = app.VehicleType,
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Application approved for {app.FullName}";
            }

            return RedirectToPage();
        }

        // REJECT DRIVER
        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var app = await _context.DriverApplications.FindAsync(id);

            if (app != null)
            {
                app.Status = "Rejected";
                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] = $"Application rejected for {app.FullName}";
            }

            return RedirectToPage();
        }
    }
}