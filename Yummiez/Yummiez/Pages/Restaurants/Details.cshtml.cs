using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Yummiez.Data;
using Yummiez.Helpers;
using Yummiez.Models;
using Yummiez.Services;

namespace Yummiez.Pages.Restaurants
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly YummiezDbContext _context;
        private readonly GeocodingService _geocodingService;

        public DetailsModel(YummiezDbContext context, GeocodingService geocodingService)
        {
            _context = context;
            _geocodingService = geocodingService;
        }

        public Restaurant Restaurant { get; set; } = default!;
        public List<RestaurantMenuCatalog.MenuItemOption> MenuItems { get; set; } = new();

        [BindProperty]
        [Required(ErrorMessage = "Please enter a delivery address.")]
        [StringLength(250, MinimumLength = 5)]
        public string DeliveryAddress { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is not int rid || rid <= 0)
                return NotFound();

            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(m => m.RestaurantId == rid);
            if (restaurant is not null)
            {
                Restaurant = restaurant;
                MenuItems = RestaurantMenuCatalog.GetMenuItems(restaurant).ToList();
                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostOrderAsync(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            if (!User.IsInRole("User"))
            {
                TempData["ErrorMessage"] = "Only user accounts can place orders.";
                return RedirectToPage(new { id });
            }

            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(m => m.RestaurantId == id);
            if (restaurant == null)
                return NotFound();

            Restaurant = restaurant;
            MenuItems = RestaurantMenuCatalog.GetMenuItems(restaurant).ToList();

            DeliveryAddress = DeliveryAddress.Trim();
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var restaurantCoords = await _geocodingService.TryGeocodeAsync(restaurant.Address);
            if (restaurantCoords == null)
            {
                TempData["ErrorMessage"] = "Could not resolve restaurant location. Please try again later.";
                return RedirectToPage(new { id });
            }

            var destCoords = await _geocodingService.TryGeocodeAsync(DeliveryAddress);
            if (destCoords == null)
            {
                ModelState.AddModelError("DeliveryAddress", "Please enter a valid delivery address.");
                return Page();
            }

            string[] driverNames = { "Alex M.", "Jordan K.", "Taylor R.", "Casey P.", "Morgan L." };
            var driverName = driverNames[new Random().Next(driverNames.Length)];

            var order = new Order
            {
                UserId = userId!,
                RestaurantId = restaurant.RestaurantId,
                DeliveryAddress = DeliveryAddress,
                Status = OrderStatus.Placed,
                RestaurantLat = restaurantCoords.Value.lat,
                RestaurantLng = restaurantCoords.Value.lng,
                DriverLat = restaurantCoords.Value.lat,
                DriverLng = restaurantCoords.Value.lng,
                DestLat = destCoords.Value.lat,
                DestLng = destCoords.Value.lng,
                DriverName = driverName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Orders/Track", new { id = order.OrderId });
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int id, string itemName, decimal unitPrice)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            if (!User.IsInRole("User"))
            {
                TempData["ErrorMessage"] = "Only user accounts can add items to cart.";
                return RedirectToPage(new { id });
            }

            itemName = itemName?.Trim() ?? string.Empty;
            if (itemName.Length is < 1 or > 120)
            {
                TempData["ErrorMessage"] = "Invalid menu item.";
                return RedirectToPage(new { id });
            }

            if (unitPrice is < 0.01m or > 10_000m)
            {
                TempData["ErrorMessage"] = "Invalid item price.";
                return RedirectToPage(new { id });
            }

            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(m => m.RestaurantId == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            if (restaurant.IsOpen != true)
            {
                TempData["ErrorMessage"] = "This restaurant is currently closed.";
                return RedirectToPage(new { id });
            }

            if (!RestaurantMenuCatalog.IsValidMenuLine(restaurant, itemName, unitPrice))
            {
                TempData["ErrorMessage"] = "Invalid menu selection.";
                return RedirectToPage(new { id });
            }

            var cart = CartSessionHelper.GetCart(HttpContext.Session);
            if (cart.Any() && cart.Any(i => i.RestaurantId != id))
            {
                TempData["ErrorMessage"] = "Please checkout or clear your current cart before ordering from another restaurant.";
                return RedirectToPage(new { id });
            }

            var existing = cart.FirstOrDefault(i =>
                i.RestaurantId == id &&
                i.ItemName == itemName &&
                i.UnitPrice == unitPrice);

            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    RestaurantId = id,
                    RestaurantName = restaurant.Name,
                    ItemName = itemName,
                    UnitPrice = unitPrice,
                    Quantity = 1
                });
            }

            CartSessionHelper.SaveCart(HttpContext.Session, cart);
            TempData["SuccessMessage"] = $"{itemName} added to cart.";
            return RedirectToPage(new { id });
        }

    }
}
