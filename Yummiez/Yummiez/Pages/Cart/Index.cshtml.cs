using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Yummiez.Data;
using Yummiez.Helpers;
using Yummiez.Models;
using Yummiez.Services;

namespace Yummiez.Pages.Cart
{
    [Authorize(Roles = "User")]
    public class IndexModel : PageModel
    {
        private readonly YummiezDbContext _context;
        private readonly GeocodingService _geocodingService;

        public IndexModel(YummiezDbContext context, GeocodingService geocodingService)
        {
            _context = context;
            _geocodingService = geocodingService;
        }

        public List<CartItem> Items { get; set; } = new();
        public decimal CartTotal => Items.Sum(i => i.TotalPrice);

        [BindProperty]
        [Required]
        [StringLength(250, MinimumLength = 5)]
        public string DeliveryAddress { get; set; } = string.Empty;

        public void OnGet()
        {
            Items = CartSessionHelper.GetCart(HttpContext.Session);
        }

        public IActionResult OnPostRemove(int index)
        {
            if (index < 0)
            {
                return RedirectToPage();
            }

            var items = CartSessionHelper.GetCart(HttpContext.Session);
            if (index >= 0 && index < items.Count)
            {
                items.RemoveAt(index);
                CartSessionHelper.SaveCart(HttpContext.Session, items);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostClear()
        {
            CartSessionHelper.SaveCart(HttpContext.Session, new List<CartItem>());
            TempData["SuccessMessage"] = "Cart cleared.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            var items = CartSessionHelper.GetCart(HttpContext.Session);
            if (!items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToPage();
            }

            DeliveryAddress = DeliveryAddress.Trim();
            if (!ModelState.IsValid)
            {
                Items = items;
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var restaurantId = items[0].RestaurantId;
            if (restaurantId <= 0 || items.Any(i => i.RestaurantId != restaurantId))
            {
                TempData["ErrorMessage"] = "Your cart is invalid. Please clear it and try again.";
                return RedirectToPage();
            }

            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);
            if (restaurant == null)
            {
                TempData["ErrorMessage"] = "Restaurant not found for this cart.";
                return RedirectToPage();
            }

            foreach (var line in items)
            {
                if (line.Quantity is < 1 or > 50)
                {
                    TempData["ErrorMessage"] = "Invalid cart quantities.";
                    return RedirectToPage();
                }

                if (!string.Equals(line.RestaurantName, restaurant.Name, StringComparison.Ordinal))
                {
                    TempData["ErrorMessage"] = "Your cart is out of date. Please clear it and try again.";
                    return RedirectToPage();
                }

                if (!RestaurantMenuCatalog.IsValidMenuLine(restaurant, line.ItemName, line.UnitPrice))
                {
                    TempData["ErrorMessage"] = "Invalid cart items detected. Please clear your cart and try again.";
                    return RedirectToPage();
                }
            }

            var restaurantCoords = await _geocodingService.TryGeocodeAsync(restaurant.Address);
            if (restaurantCoords == null)
            {
                TempData["ErrorMessage"] = "Could not resolve restaurant location. Please try again later.";
                return RedirectToPage();
            }

            var destCoords = await _geocodingService.TryGeocodeAsync(DeliveryAddress);
            if (destCoords == null)
            {
                ModelState.AddModelError("DeliveryAddress", "Please enter a valid delivery address.");
                Items = items;
                return Page();
            }
            string[] driverNames = { "Alex M.", "Jordan K.", "Taylor R.", "Casey P.", "Morgan L." };
            var driverName = driverNames[new Random().Next(driverNames.Length)];

            var order = new Order
            {
                UserId = userId,
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

            CartSessionHelper.SaveCart(HttpContext.Session, new List<CartItem>());
            TempData["SuccessMessage"] = "Order placed successfully.";
            return RedirectToPage("/Orders/Track", new { id = order.OrderId });
        }

    }
}
