using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages.Driver
{
    [Authorize]
    public class ApplyModel : PageModel
    {
        private readonly YummiezDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ApplyModel(YummiezDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public DriverApplication Application { get; set; } = new();

        public string? CurrentStatus { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return;
            }

            var app = await _context.DriverApplications
                .FirstOrDefaultAsync(a => a.UserId == user.Id);

            if (app != null)
            {
                CurrentStatus = app.Status;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Application.VehicleType = (Application.VehicleType ?? string.Empty).Trim();
            Application.FullName = (Application.FullName ?? string.Empty).Trim();
            Application.LicenseNumber = (Application.LicenseNumber ?? string.Empty).Trim();
            Application.VehicleInfo = (Application.VehicleInfo ?? string.Empty).Trim();

            var allowedVehicleTypes = new[] { "Car", "Bike", "Scooter" };
            ModelState.Remove("Application.VehicleType");
            if (!allowedVehicleTypes.Contains(Application.VehicleType, StringComparer.Ordinal))
            {
                ModelState.AddModelError("Application.VehicleType", "Please select a valid vehicle type.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill out all required fields.";
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            // Prevent duplicate applications
            var existing = await _context.DriverApplications
                .FirstOrDefaultAsync(a => a.UserId == user.Id);

            if (existing != null)
            {
                TempData["ErrorMessage"] = $"You already applied. Status: {existing.Status}";
                return RedirectToPage("/Driver/Apply");
            }

            Application.UserId = user.Id;
            Application.Status = "Pending";

            _context.DriverApplications.Add(Application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Application submitted! Status: Pending approval.";

            return RedirectToPage("/Driver/Apply");
        }
    }
}