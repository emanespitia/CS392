using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Yummiez.Data;
using Yummiez.Helpers;
using Yummiez.Models;

namespace Yummiez.Pages.Restaurants
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly YummiezDbContext _context;

        public DetailsModel(YummiezDbContext context)
        {
            _context = context;
        }

        public Restaurant Restaurant { get; set; } = default!;
        public List<MenuItemOption> MenuItems { get; set; } = new();

        [BindProperty]
        public string? DeliveryAddress { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(m => m.RestaurantId == id);
            if (restaurant is not null)
            {
                Restaurant = restaurant;
                MenuItems = BuildMenuItems(restaurant);
                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostOrderAsync(int id)
        {
            if (!User.IsInRole("User"))
            {
                TempData["ErrorMessage"] = "Only user accounts can place orders.";
                return RedirectToPage(new { id });
            }

            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(m => m.RestaurantId == id);
            if (restaurant == null)
                return NotFound();

            Restaurant = restaurant;
            MenuItems = BuildMenuItems(restaurant);

            if (string.IsNullOrWhiteSpace(DeliveryAddress))
            {
                ModelState.AddModelError("DeliveryAddress", "Please enter a delivery address.");
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Simulated coordinates for Newark, NJ area restaurants
            var restaurantCoords = GetRestaurantCoords(restaurant.Address);
            var destCoords = GetDestinationCoords(DeliveryAddress);

            string[] driverNames = { "Alex M.", "Jordan K.", "Taylor R.", "Casey P.", "Morgan L." };
            var driverName = driverNames[new Random().Next(driverNames.Length)];

            var order = new Order
            {
                UserId = userId!,
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

            return RedirectToPage("/Orders/Track", new { id = order.OrderId });
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int id, string itemName, decimal unitPrice)
        {
            if (!User.IsInRole("User"))
            {
                TempData["ErrorMessage"] = "Only user accounts can add items to cart.";
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

        public class MenuItemOption
        {
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
        }

        private static List<MenuItemOption> BuildMenuItems(Restaurant restaurant)
        {
            var category = restaurant.Category?.ToLowerInvariant() ?? string.Empty;
            return category switch
            {
                "pizza" => new List<MenuItemOption>
                {
                    new() { Name = "Margherita Pizza", Price = 13.99m },
                    new() { Name = "Pepperoni Pizza", Price = 15.49m },
                    new() { Name = "Garlic Knots", Price = 5.99m }
                },
                "sushi" => new List<MenuItemOption>
                {
                    new() { Name = "California Roll", Price = 11.99m },
                    new() { Name = "Salmon Nigiri (6pc)", Price = 14.99m },
                    new() { Name = "Miso Soup", Price = 4.49m }
                },
                "mexican" => new List<MenuItemOption>
                {
                    new() { Name = "Chicken Tacos", Price = 10.99m },
                    new() { Name = "Burrito Bowl", Price = 12.49m },
                    new() { Name = "Chips & Guac", Price = 6.99m }
                },
                "healthy" => new List<MenuItemOption>
                {
                    new() { Name = "Quinoa Bowl", Price = 11.49m },
                    new() { Name = "Avocado Salad", Price = 9.99m },
                    new() { Name = "Protein Smoothie", Price = 7.99m }
                },
                _ => new List<MenuItemOption>
                {
                    new() { Name = "Classic Burger", Price = 12.99m },
                    new() { Name = "Crispy Fries", Price = 4.99m },
                    new() { Name = "Soft Drink", Price = 2.99m }
                }
            };
        }

        // Simulated coordinates based on address keywords
        private (double lat, double lng) GetRestaurantCoords(string address)
        {
            if (address.Contains("123 Main")) return (40.7357, -74.1724);
            if (address.Contains("456 Broad")) return (40.7395, -74.1712);
            if (address.Contains("789 Market")) return (40.7340, -74.1680);
            if (address.Contains("321 Park")) return (40.7410, -74.1760);
            if (address.Contains("654 University")) return (40.7450, -74.1800);
            // Default: center of Newark
            return (40.7357 + new Random().NextDouble() * 0.01, -74.1724 + new Random().NextDouble() * 0.01);
        }

        private (double lat, double lng) GetDestinationCoords(string address)
        {
            // Generate a point ~2-3km away from center of Newark for a realistic delivery
            var rng = new Random();
            double baseLat = 40.7357;
            double baseLng = -74.1724;
            return (baseLat + 0.01 + rng.NextDouble() * 0.015, baseLng + 0.01 + rng.NextDouble() * 0.015);
        }
    }
}
