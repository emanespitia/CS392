using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Yummiez.Data;
using Yummiez.Models;

[Authorize(Roles = "Driver")]
public class DashboardModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly YummiezDbContext _db;

    public DashboardModel(UserManager<IdentityUser> userManager, YummiezDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public Driver Driver { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return;
        }

        Driver = _db.Drivers
            .FirstOrDefault(d => d.IdentityUserId == user.Id);
    }
}