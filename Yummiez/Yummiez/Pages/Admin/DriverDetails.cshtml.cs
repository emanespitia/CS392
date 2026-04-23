using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DriverDetailsModel : PageModel
{
    private readonly YummiezDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DriverDetailsModel(YummiezDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public Yummiez.Models.Driver Driver { get; set; } = null!;
    public IdentityUser? Account { get; set; }
    public UserProfile? Profile { get; set; }
    public DriverApplication? Application { get; set; }

    public async Task<IActionResult> OnGetAsync(int driverId)
    {
        if (driverId <= 0)
        {
            return RedirectToPage("/Admin/Index");
        }

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.DriverId == driverId);
        if (driver == null)
        {
            return RedirectToPage("/Admin/Index");
        }

        Driver = driver;
        Account = await _userManager.FindByIdAsync(driver.IdentityUserId);
        Profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == driver.IdentityUserId);
        Application = await _context.DriverApplications
            .Where(a => a.UserId == driver.IdentityUserId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        return Page();
    }
}
