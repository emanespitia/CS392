using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Helpers;
using Yummiez.Models;

namespace Yummiez.Pages.Manager
{
    [Authorize(Roles = "Manager")]
    public class IndexModel : PageModel
    {
        private readonly YummiezDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(YummiezDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Restaurant? Restaurant { get; set; }
        public List<Order> Orders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            Restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.ManagerUserId == user.Id);

            if (Restaurant == null)
            {
                return Page();
            }

            Orders = await _context.Orders
                .Where(o => o.RestaurantId == Restaurant.RestaurantId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAcceptOrderAsync(int orderId)
        {
            if (!InputValidation.IsValidPositiveOrderId(orderId))
            {
                TempData["ErrorMessage"] = "Invalid order.";
                return RedirectToPage();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var order = await _context.Orders.Include(o => o.Restaurant).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order?.Restaurant == null || order.Restaurant.ManagerUserId != user.Id)
            {
                TempData["ErrorMessage"] = "You are not allowed to manage this order.";
                return RedirectToPage();
            }

            if (order.Status == OrderStatus.Placed)
            {
                order.Status = OrderStatus.Accepted;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Order #{order.OrderId} accepted.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkReadyAsync(int orderId)
        {
            if (!InputValidation.IsValidPositiveOrderId(orderId))
            {
                TempData["ErrorMessage"] = "Invalid order.";
                return RedirectToPage();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var order = await _context.Orders.Include(o => o.Restaurant).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order?.Restaurant == null || order.Restaurant.ManagerUserId != user.Id)
            {
                TempData["ErrorMessage"] = "You are not allowed to manage this order.";
                return RedirectToPage();
            }

            if (order.Status == OrderStatus.Accepted)
            {
                order.Status = OrderStatus.Ready;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Order #{order.OrderId} marked ready for pickup.";
            }

            return RedirectToPage();
        }
    }
}
