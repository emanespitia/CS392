using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderTrackingController : ControllerBase
    {
        private readonly YummiezDbContext _context;
        private const int TotalSteps = 10;

        public OrderTrackingController(YummiezDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}/track")]
        public async Task<IActionResult> Track(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            // Only allow the owner to track their order
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (order.UserId != userId)
                return Forbid();

            // If already delivered, return current state
            if (order.Status == OrderStatus.Delivered)
            {
                return Ok(new
                {
                    order.DriverLat,
                    order.DriverLng,
                    order.DestLat,
                    order.DestLng,
                    order.RestaurantLat,
                    order.RestaurantLng,
                    order.Status,
                    order.DriverName,
                    order.StepCount,
                    TotalSteps,
                    RestaurantName = order.Restaurant?.Name ?? "Restaurant"
                });
            }

            // Advance the driver one step closer
            order.StepCount++;

            // Determine status based on progress
            if (order.StepCount <= 2)
                order.Status = OrderStatus.Preparing;
            else if (order.StepCount <= 3)
                order.Status = OrderStatus.PickedUp;
            else if (order.StepCount < TotalSteps)
                order.Status = OrderStatus.OnTheWay;
            else
            {
                order.Status = OrderStatus.Delivered;
                order.DeliveredAt = DateTime.UtcNow;
            }

            // Interpolate driver position from restaurant to destination
            double progress = Math.Min((double)order.StepCount / TotalSteps, 1.0);
            order.DriverLat = order.RestaurantLat + (order.DestLat - order.RestaurantLat) * progress;
            order.DriverLng = order.RestaurantLng + (order.DestLng - order.RestaurantLng) * progress;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                order.DriverLat,
                order.DriverLng,
                order.DestLat,
                order.DestLng,
                order.RestaurantLat,
                order.RestaurantLng,
                order.Status,
                order.DriverName,
                order.StepCount,
                TotalSteps,
                RestaurantName = order.Restaurant?.Name ?? "Restaurant"
            });
        }
    }
}
