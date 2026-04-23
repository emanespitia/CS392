using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Models;
using Yummiez.Constants;

namespace Yummiez.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ApplicationDetailsModel : PageModel
    {
        private readonly YummiezDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ApplicationDetailsModel(YummiezDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public DriverApplication? Application { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var application = await _context.DriverApplications.FindAsync(id);

            if (application == null)
                return RedirectToPage("/Admin/Index");

            Application = application;
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var app = await _context.DriverApplications.FindAsync(id);

            if (app != null && app.Status == "Pending")
            {
                app.Status = "Approved";

                var user = await _userManager.FindByIdAsync(app.UserId);

                if (user != null)
                {
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
                            UserId = app.UserId,
                            IdentityUserId = app.UserId,
                            LicenseNumber = app.LicenseNumber,
                            VehicleType = app.VehicleType,
                            IsAvailable = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Admin/Index");
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var app = await _context.DriverApplications.FindAsync(id);

            if (app != null)
            {
                app.Status = "Rejected";
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Admin/Index");
        }
    }
}
