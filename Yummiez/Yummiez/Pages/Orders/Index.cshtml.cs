using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages.Orders
{
    [Authorize(Roles = "User")]
    public class IndexModel : PageModel
    {
        private readonly YummiezDbContext _context;

        public IndexModel(YummiezDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Orders = await _context.Orders
                .Include(o => o.Restaurant)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}
