using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages.Restaurants
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly Yummiez.Data.YummiezDbContext _context;

        public IndexModel(Yummiez.Data.YummiezDbContext context)
        {
            _context = context;
        }

        public IList<Restaurant> Restaurant { get;set; } = default!;
        public string? CurrentUserId { get; set; }

        public async Task OnGetAsync()
        {
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Restaurant = await _context.Restaurants.ToListAsync();
        }
    }
}
