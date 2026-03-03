using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages.Restaurants
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly Yummiez.Data.YummiezDbContext _context;

        public DetailsModel(Yummiez.Data.YummiezDbContext context)
        {
            _context = context;
        }

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
    }
}
