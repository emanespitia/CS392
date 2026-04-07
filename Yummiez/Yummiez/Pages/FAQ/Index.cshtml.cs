using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Yummiez.Models;
using Yummiez.Services;

namespace Yummiez.Pages.FAQ;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly FaqService _faqService;

    public IndexModel(FaqService faqService)
    {
        _faqService = faqService;
    }

    public List<FaqItem> FaqItems { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public bool MongoError { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Categories = await _faqService.GetCategoriesAsync();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                FaqItems = await _faqService.SearchAsync(Search);
            }
            else if (!string.IsNullOrWhiteSpace(Category))
            {
                FaqItems = await _faqService.GetByCategoryAsync(Category);
            }
            else
            {
                FaqItems = await _faqService.GetAllPublishedAsync();
            }
        }
        catch
        {
            MongoError = true;
        }
    }
}
