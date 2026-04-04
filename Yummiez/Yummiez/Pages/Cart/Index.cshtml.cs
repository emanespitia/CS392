using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Yummiez.Data;
using Yummiez.Helpers;
using Yummiez.Models;

namespace Yummiez.Pages.Cart
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly YummiezDbContext _context;

        public IndexModel(YummiezDbContext context)
        {
            _context = context;
        }

        public List<CartItem> Items { get; set; } = new();
        public decimal CartTotal => Items.Sum(i => i.TotalPrice);

        [BindProperty]
        [Required]
        [StringLength(250)]
        public string DeliveryAddress { get; set; } = string.Empty;

        public void OnGet()
        {
            Items = CartSessionHelper.GetCart(HttpContext.Session);
        }

        public IActionResult OnPostRemove(int index)
        {
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
            if (User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Admins cannot place orders.";
                return RedirectToPage("/Admin/Index");
            }

            var items = CartSessionHelper.GetCart(HttpContext.Session);
            if (!items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToPage();
            }

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
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);
            if (restaurant == null)
            {
                TempData["ErrorMessage"] = "Restaurant not found for this cart.";
                return RedirectToPage();
            }

            var restaurantCoords = GetRestaurantCoords(restaurant.Address);
            var destCoords = GetDestinationCoords(DeliveryAddress);
            string[] driverNames = { "Alex M.", "Jordan K.", "Taylor R.", "Casey P.", "Morgan L." };
            var driverName = driverNames[new Random().Next(driverNames.Length)];

            var order = new Order
            {
                UserId = userId,
                RestaurantId = restaurant.RestaurantId,
                DeliveryAddress = DeliveryAddress,
                Status = OrderStatus.Placed,
                RestaurantLat = restaurantCoords.lat,
                RestaurantLng = restaurantCoords.lng,
                DriverLat = restaurantCoords.lat,
                DriverLng = restaurantCoords.lng,
                DestLat = destCoords.lat,
                DestLng = destCoords.lng,
                DriverName = driverName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            CartSessionHelper.SaveCart(HttpContext.Session, new List<CartItem>());
            TempData["SuccessMessage"] = "Order placed successfully.";
            return RedirectToPage("/Orders/Track", new { id = order.OrderId });
        }

        private static (double lat, double lng) GetRestaurantCoords(string address)
        {
            if (address.Contains("123 Main")) return (40.7357, -74.1724);
            if (address.Contains("456 Broad")) return (40.7395, -74.1712);
            if (address.Contains("789 Market")) return (40.7340, -74.1680);
            if (address.Contains("321 Park")) return (40.7410, -74.1760);
            if (address.Contains("654 University")) return (40.7450, -74.1800);
            return (40.7357 + new Random().NextDouble() * 0.01, -74.1724 + new Random().NextDouble() * 0.01);
        }

        private static (double lat, double lng) GetDestinationCoords(string address)
        {
            var rng = new Random();
            const double baseLat = 40.7357;
            const double baseLng = -74.1724;
            return (baseLat + 0.01 + rng.NextDouble() * 0.015, baseLng + 0.01 + rng.NextDouble() * 0.015);
        }
    }
}
