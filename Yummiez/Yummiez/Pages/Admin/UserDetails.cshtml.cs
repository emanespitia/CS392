using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Helpers;

namespace Yummiez.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UserDetailsModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly YummiezDbContext _context;

    public UserDetailsModel(UserManager<IdentityUser> userManager, YummiezDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public IdentityUser Account { get; set; } = null!;
    public UserProfile? Profile { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();

    public async Task<IActionResult> OnGetAsync(string userId)
    {
        if (!InputValidation.IsValidIdentityUserId(userId))
        {
            return RedirectToPage("/Admin/Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return RedirectToPage("/Admin/Index");
        }

        Account = user;
        Roles = await _userManager.GetRolesAsync(user);
        Profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
        return Page();
    }
}
