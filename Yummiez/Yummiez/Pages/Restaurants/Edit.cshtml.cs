using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages.Restaurants
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly Yummiez.Data.YummiezDbContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(Yummiez.Data.YummiezDbContext context, ILogger<EditModel> logger)
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

            var restaurant =  await _context.Restaurants.FirstOrDefaultAsync(m => m.RestaurantId == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            Restaurant = restaurant;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingRestaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.RestaurantId == Restaurant.RestaurantId);
            if (existingRestaurant == null)
            {
                return NotFound();
            }

            existingRestaurant.Name = Restaurant.Name;
            existingRestaurant.OwnerName = Restaurant.OwnerName;
            existingRestaurant.Address = Restaurant.Address;
            existingRestaurant.Phone = Restaurant.Phone;
            existingRestaurant.IsOpen = Restaurant.IsOpen;
            existingRestaurant.Category = Restaurant.Category;
            existingRestaurant.ImageUrl = Restaurant.ImageUrl;
            existingRestaurant.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Restaurant edited: ID={Id} by {User}", Restaurant.RestaurantId, User.Identity?.Name);
                TempData["SuccessMessage"] = $"Restaurant '{Restaurant.Name}' was updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RestaurantExists(Restaurant.RestaurantId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool RestaurantExists(int id)
        {
            return _context.Restaurants.Any(e => e.RestaurantId == id);
        }
    }
}
