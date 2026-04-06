using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public DriverApplication Application { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            Application.UserId = user.Id;
            Application.Status = "Pending";

            _context.DriverApplications.Add(Application);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }
    }
}
