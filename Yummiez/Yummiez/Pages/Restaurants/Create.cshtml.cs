using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages.Restaurants
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly Yummiez.Data.YummiezDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(Yummiez.Data.YummiezDbContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Restaurant Restaurant { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Restaurant.CreatedAt = DateTime.UtcNow;
            Restaurant.AdminId = 0;
            _context.Restaurants.Add(Restaurant);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Restaurant created: {Name} by {User}", Restaurant.Name, User.Identity?.Name);
            TempData["SuccessMessage"] = $"Restaurant '{Restaurant.Name}' was created successfully!";

            return RedirectToPage("./Index");
        }
    }
}
