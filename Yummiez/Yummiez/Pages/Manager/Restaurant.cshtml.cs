using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Yummiez.Data;

namespace Yummiez.Pages.Manager
{
    [Authorize(Roles = "Manager")]
    public class RestaurantModel : PageModel
    {
        private readonly YummiezDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RestaurantModel(YummiezDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public EditRestaurantInput Input { get; set; } = new();

        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;

        public class EditRestaurantInput
        {
            [Required]
            [StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [Required]
            [StringLength(500)]
            public string Address { get; set; } = string.Empty;

            [StringLength(20)]
            public string? Phone { get; set; }

            [StringLength(50)]
            public string? Category { get; set; }

            [StringLength(500)]
            public string? ImageUrl { get; set; }

            public bool IsOpen { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.ManagerUserId == user.Id);
            if (restaurant == null)
            {
                TempData["ErrorMessage"] = "You are not assigned to a restaurant.";
                return RedirectToPage("/Manager/Index");
            }

            RestaurantId = restaurant.RestaurantId;
            RestaurantName = restaurant.Name;
            Input = new EditRestaurantInput
            {
                Name = restaurant.Name,
                Address = restaurant.Address,
                Phone = restaurant.Phone,
                Category = restaurant.Category,
                ImageUrl = restaurant.ImageUrl,
                IsOpen = restaurant.IsOpen == true
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.ManagerUserId == user.Id);
            if (restaurant == null)
            {
                TempData["ErrorMessage"] = "You are not assigned to a restaurant.";
                return RedirectToPage("/Manager/Index");
            }

            RestaurantId = restaurant.RestaurantId;
            RestaurantName = restaurant.Name;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            restaurant.Name = Input.Name;
            restaurant.Address = Input.Address;
            restaurant.Phone = Input.Phone;
            restaurant.Category = Input.Category;
            restaurant.ImageUrl = Input.ImageUrl;
            restaurant.IsOpen = Input.IsOpen;
            restaurant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Restaurant details updated.";
            return RedirectToPage("/Manager/Index");
        }
    }
}
