using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Pages.TestRecords
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(ApplicationDbContext context, ILogger<DeleteModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public TestEliasMissaEM TestEliasMissaEM { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var record = await _context.TestEliasMissaEM.FirstOrDefaultAsync(m => m.Id == id);

            if (record is not null)
            {
                TestEliasMissaEM = record;
                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var record = await _context.TestEliasMissaEM.FindAsync(id);
            if (record != null)
            {
                TestEliasMissaEM = record;
                _context.TestEliasMissaEM.Remove(TestEliasMissaEM);
                await _context.SaveChangesAsync();
                _logger.LogInformation("TestRecord deleted: ID={Id}, Name={Name} by {User}", id, TestEliasMissaEM.ProjectName, User.Identity?.Name);
            }

            return RedirectToPage("./Index");
        }
    }
}
