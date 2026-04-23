using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Models;
using System.ComponentModel.DataAnnotations;

[Authorize]
public class SettingsModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly YummiezDbContext _db;

    public SettingsModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        YummiezDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    public string Email { get; set; } = string.Empty;
    public UserProfile Profile { get; set; } = new();
    public Driver? Driver { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string? NewEmail { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number")]
        public string? Phone { get; set; }

        public string? VehicleType { get; set; }

        [StringLength(50)]
        public string? LicenseNumber { get; set; }
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        Email = user.Email ?? "";

        // PROFILE
        Profile = await _db.UserProfiles
            .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id)
            ?? new UserProfile { IdentityUserId = user.Id };

        if (Profile.Id == 0)
        {
            Profile.FullName = Profile.FullName ?? string.Empty;
            Profile.PhoneNumber = Profile.PhoneNumber ?? string.Empty;
            _db.UserProfiles.Add(Profile);
            await _db.SaveChangesAsync();
        }

        // DRIVER
        Driver = await _db.Drivers
            .FirstOrDefaultAsync(d => d.IdentityUserId == user.Id);

        // LOAD INPUT
        Input = new InputModel
        {
            FullName = Profile.FullName,
            Phone = Profile.PhoneNumber,
            VehicleType = Driver?.VehicleType,
            LicenseNumber = Driver?.LicenseNumber
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToPage();
        }

        var isDriver = await _userManager.IsInRoleAsync(user, "Driver");

        //  FIX: remove driver validation if not driver
        if (!isDriver)
        {
            ModelState.Remove("Input.VehicleType");
            ModelState.Remove("Input.LicenseNumber");
        }

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        // PROFILE
        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

        if (profile == null)
        {
            profile = new UserProfile
            {
                IdentityUserId = user.Id
            };
            _db.UserProfiles.Add(profile);
        }

        profile.FullName = Input.FullName.Trim();
        profile.PhoneNumber = (Input.Phone ?? string.Empty).Trim();

        // 🔥 EMAIL + USERNAME FIX (CRITICAL)
        if (!string.IsNullOrEmpty(Input.NewEmail) && Input.NewEmail != user.Email)
        {
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
            var result = await _userManager.ChangeEmailAsync(user, Input.NewEmail, token);

            if (result.Succeeded)
            {
                // keep login consistent
                await _userManager.SetUserNameAsync(user, Input.NewEmail);

                // refresh cookie → navbar updates instantly
                await _signInManager.RefreshSignInAsync(user);
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update email.";
                return RedirectToPage();
            }
        }

        // DRIVER
        if (isDriver)
        {
            var driver = await _db.Drivers
                .FirstOrDefaultAsync(d => d.IdentityUserId == user.Id);

            if (driver != null)
            {
                driver.VehicleType = Input.VehicleType;
                driver.LicenseNumber = Input.LicenseNumber;
            }
        }

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Settings updated successfully!";
        return RedirectToPage();
    }
}