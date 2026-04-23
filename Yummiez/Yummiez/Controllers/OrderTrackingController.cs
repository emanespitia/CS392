using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Helpers;
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
            if (!InputValidation.IsValidPositiveOrderId(id))
            {
                return BadRequest();
            }

            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            // Only allow the owner to track their order
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (order.UserId != userId)
                return Forbid();

            var driverName = order.DriverName;
            if (!string.IsNullOrWhiteSpace(order.DriverUserId))
            {
                var profileName = await _context.UserProfiles
                    .Where(p => p.IdentityUserId == order.DriverUserId)
                    .Select(p => p.FullName)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(profileName))
                {
                    driverName = profileName.Trim();
                }
            }

            return Ok(new
            {
                order.DriverLat,
                order.DriverLng,
                order.DestLat,
                order.DestLng,
                order.RestaurantLat,
                order.RestaurantLng,
                order.Status,
                DriverName = driverName,
                order.StepCount,
                TotalSteps,
                RestaurantName = order.Restaurant?.Name ?? "Restaurant"
            });
        }

        [HttpPost("{id}/driver-location")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> UpdateDriverLocation(int id, [FromBody] DriverLocationRequest? request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!InputValidation.IsValidPositiveOrderId(id))
            {
                return BadRequest("Invalid order id.");
            }

            if (!InputValidation.IsValidLatitudeLongitude(request.Lat, request.Lng))
            {
                return BadRequest("Invalid coordinates.");
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Forbid();
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.DriverUserId != userId)
            {
                return Forbid();
            }

            if (order.Status == OrderStatus.Delivered)
            {
                return BadRequest("Order is already delivered.");
            }

            order.DriverLat = request.Lat;
            order.DriverLng = request.Lng;
            if (order.Status == OrderStatus.PickedUp)
            {
                order.Status = OrderStatus.OnTheWay;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class DriverLocationRequest
    {
        [Range(-90, 90)]
        public double Lat { get; set; }

        [Range(-180, 180)]
        public double Lng { get; set; }
    }
}
