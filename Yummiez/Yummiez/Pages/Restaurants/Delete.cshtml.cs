using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages.Restaurants
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly Yummiez.Data.YummiezDbContext _context;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(Yummiez.Data.YummiezDbContext context, ILogger<DeleteModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Restaurant Restaurant { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(m => m.RestaurantId == id);

            if (restaurant is not null)
            {
                Restaurant = restaurant;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant != null)
            {
                Restaurant = restaurant;
                _context.Restaurants.Remove(Restaurant);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Restaurant deleted: ID={Id}, Name={Name} by {User}", id, Restaurant.Name, User.Identity?.Name);
            }

            return RedirectToPage("./Index");
        }
    }
}
