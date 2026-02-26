using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Yummiez.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserWithRole> Users { get; set; } = new();

        public class UserWithRole
        {
            public string Id { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string Role { get; set; } = null!;
        }

        public async Task OnGetAsync()
        {
            var allUsers = await _userManager.Users.ToListAsync();
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

        public async Task<IActionResult> OnPostPromoteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                if (await _userManager.IsInRoleAsync(user, "User"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "User");
                }
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDemoteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Prevent demoting the last admin
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count > 1)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await _userManager.RemoveFromRoleAsync(user, "Admin");
                    }
                    if (!await _userManager.IsInRoleAsync(user, "User"))
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                }
            }
            return RedirectToPage();
        }
    }
}
