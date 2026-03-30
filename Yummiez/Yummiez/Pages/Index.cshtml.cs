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

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

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

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var term = SearchTerm.ToLower();
                query = query.Where(r => r.Name.ToLower().Contains(term) || r.Address.ToLower().Contains(term));
            }

            Restaurants = await query.ToListAsync();
        }
    }
}
