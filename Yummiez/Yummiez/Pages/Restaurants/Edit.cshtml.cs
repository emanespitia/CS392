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
using Yummiez.Helpers;
using Yummiez.Models;
using System.Security.Claims;

namespace Yummiez.Pages.Restaurants
{
    [Authorize(Roles = "Admin,Manager")]
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

        [BindProperty]
        public string MenuItemsInput { get; set; } = string.Empty;

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

            if (User.IsInRole("Manager"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId) || restaurant.ManagerUserId != userId)
                {
                    return Forbid();
                }
            }

            Restaurant = restaurant;
            MenuItemsInput = RestaurantMenuCatalog.FormatMenuForEditor(restaurant);
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

            if (User.IsInRole("Manager"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId) || existingRestaurant.ManagerUserId != userId)
                {
                    return Forbid();
                }
            }

            existingRestaurant.Name = Restaurant.Name;
            existingRestaurant.OwnerName = Restaurant.OwnerName;
            existingRestaurant.Address = Restaurant.Address;
            existingRestaurant.Phone = Restaurant.Phone;
            existingRestaurant.IsOpen = Restaurant.IsOpen;
            existingRestaurant.Category = Restaurant.Category;
            existingRestaurant.ImageUrl = Restaurant.ImageUrl;
            var customMenu = RestaurantMenuCatalog.ParseMenuInput(MenuItemsInput);
            existingRestaurant.MenuItemsJson = customMenu.Count > 0
                ? RestaurantMenuCatalog.SerializeMenuItems(customMenu)
                : null;
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
