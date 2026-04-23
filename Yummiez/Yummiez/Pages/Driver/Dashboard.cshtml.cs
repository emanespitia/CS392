using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Helpers;
using Yummiez.Models;

[Authorize(Roles = "Driver")]
public class DashboardModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly YummiezDbContext _db;

    public DashboardModel(UserManager<IdentityUser> userManager, YummiezDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public Driver? Driver { get; set; }
    public List<Order> AvailableOrders { get; set; } = new();
    public List<Order> MyActiveOrders { get; set; } = new();

    
    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return;
        }

        Driver = _db.Drivers
            .FirstOrDefault(d => d.IdentityUserId == user.Id);

        AvailableOrders = await _db.Orders
            .Include(o => o.Restaurant)
            .Where(o => o.Status == OrderStatus.Ready)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

        MyActiveOrders = await _db.Orders
            .Include(o => o.Restaurant)
            .Where(o => o.DriverUserId == user.Id && o.Status != OrderStatus.Delivered)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostToggleAvailabilityAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var driver = _db.Drivers
            .FirstOrDefault(d => d.IdentityUserId == user.Id);

        if (driver != null)
        {
            driver.IsAvailable = !driver.IsAvailable;
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAcceptPickupAsync(int orderId, double? driverLat, double? driverLng)
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

        if (driverLat is null || driverLng is null || !InputValidation.IsValidLatitudeLongitude(driverLat.Value, driverLng.Value))
        {
            TempData["ErrorMessage"] = "Please enable location sharing before picking up an order.";
            return RedirectToPage();
        }

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order == null || order.Status != OrderStatus.Ready)
        {
            TempData["ErrorMessage"] = "This order is not available for pickup.";
            return RedirectToPage();
        }

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
        var displayName = string.IsNullOrWhiteSpace(profile?.FullName)
            ? (user.UserName ?? user.Email ?? "Driver")
            : profile.FullName.Trim();

        order.DriverUserId = user.Id;
        order.DriverName = displayName;
        order.DriverLat = driverLat.Value;
        order.DriverLng = driverLng.Value;
        order.Status = OrderStatus.PickedUp;
        order.StepCount = 1;
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Order #{order.OrderId} picked up.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkDeliveredAsync(int orderId)
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

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId && o.DriverUserId == user.Id);
        if (order == null)
        {
            TempData["ErrorMessage"] = "Order not found for this driver.";
            return RedirectToPage();
        }

        if (order.Status == OrderStatus.PickedUp || order.Status == OrderStatus.OnTheWay)
        {
            order.Status = OrderStatus.Delivered;
            order.DeliveredAt = DateTime.UtcNow;
            order.StepCount = 10;
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Order #{order.OrderId} marked delivered.";
        }

        return RedirectToPage();
    }
}