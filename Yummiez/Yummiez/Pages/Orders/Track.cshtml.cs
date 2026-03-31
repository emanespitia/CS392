using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages.Orders
{
    [Authorize(Roles = "Admin,User")]
    public class TrackModel : PageModel
    {
        private readonly YummiezDbContext _context;

        public TrackModel(YummiezDbContext context)
        {
            _context = context;
        }

        public Order Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
                return RedirectToPage("/Orders/Index");

            Order = order;
            return Page();
        }
    }
}
