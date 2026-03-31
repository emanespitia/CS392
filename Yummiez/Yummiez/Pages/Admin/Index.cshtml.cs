using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Yummiez.Constants;
using Yummiez.Data;

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
            // Load stats
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
                TempData["SuccessMessage"] = $"User '{user.Email}' has been promoted to Admin!";

                await _userManager.AddToRoleAsync(user, role);
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
                    TempData["SuccessMessage"] = $"User '{user.Email}' has been demoted to User.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cannot demote the last admin. At least one admin must always exist.";
                }

                // Domain tables can keep foreign-key references to the Identity user.
                // Remove those dependent rows first so the Identity delete won't fail.
                // Some deployments may use either `user_id` or `identity_user_id`, so we handle both safely.
                var uid = user.Id;
                await _yummiezDbContext.Database.ExecuteSqlInterpolatedAsync($@"
                    DECLARE @uid NVARCHAR(450) = {uid};

                    -- Drivers
                    IF COL_LENGTH('dbo.Drivers', 'user_id') IS NOT NULL
                    BEGIN
                        EXEC(N'DELETE FROM dbo.Drivers WHERE user_id = @p_uid', N'@p_uid NVARCHAR(450)', @p_uid = @uid);
                    END;
                    IF COL_LENGTH('dbo.Drivers', 'identity_user_id') IS NOT NULL
                    BEGIN
                        EXEC(N'DELETE FROM dbo.Drivers WHERE identity_user_id = @p_uid', N'@p_uid NVARCHAR(450)', @p_uid = @uid);
                    END;

                    -- Clients
                    IF COL_LENGTH('dbo.Clients', 'user_id') IS NOT NULL
                    BEGIN
                        EXEC(N'DELETE FROM dbo.Clients WHERE user_id = @p_uid', N'@p_uid NVARCHAR(450)', @p_uid = @uid);
                    END;
                    IF COL_LENGTH('dbo.Clients', 'identity_user_id') IS NOT NULL
                    BEGIN
                        EXEC(N'DELETE FROM dbo.Clients WHERE identity_user_id = @p_uid', N'@p_uid NVARCHAR(450)', @p_uid = @uid);
                    END;
                ");

                await _userManager.DeleteAsync(user);
            }
            return RedirectToPage();
        }
    }
}
