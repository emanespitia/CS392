using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        public IndexModel(ILogger<IndexModel> logger, YummiezDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            var query = _context.Restaurants.AsQueryable();

            if (!string.IsNullOrEmpty(Category))
            {
                query = query.Where(r => r.Category == Category);
            }

            Restaurants = await query.ToListAsync();
        }
    }
}
