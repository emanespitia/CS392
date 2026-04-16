using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly YummiezDbContext _context;

        public IList<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
        public Restaurant? ManagerRestaurant { get; set; }
        public int ManagerTotalOrders { get; set; }
        public int ManagerPendingOrders { get; set; }
        public int ManagerReadyOrders { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public IndexModel(ILogger<IndexModel> logger, YummiezDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Manager"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    ManagerRestaurant = await _context.Restaurants
                        .FirstOrDefaultAsync(r => r.ManagerUserId == userId);

                    if (ManagerRestaurant != null)
                    {
                        var managerOrders = _context.Orders
                            .Where(o => o.RestaurantId == ManagerRestaurant.RestaurantId);
                        ManagerTotalOrders = await managerOrders.CountAsync();
                        ManagerPendingOrders = await managerOrders.CountAsync(o => o.Status == OrderStatus.Placed);
                        ManagerReadyOrders = await managerOrders.CountAsync(o => o.Status == OrderStatus.Ready);
                    }
                }

                return;
            }

            var query = _context.Restaurants.AsQueryable();

            if (!string.IsNullOrEmpty(Category))
            {
                query = query.Where(r => r.Category == Category);
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var term = SearchTerm.ToLower();
                query = query.Where(r => r.Name.ToLower().Contains(term) || r.Address.ToLower().Contains(term));
            }

            Restaurants = await query.ToListAsync();
        }
    }
}
