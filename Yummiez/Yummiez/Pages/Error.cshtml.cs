using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Yummiez.Pages;

[AllowAnonymous]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public new int? StatusCode { get; set; }

    public string ErrorTitle { get; set; } = "Oops! Something went wrong.";
    public string ErrorMessage { get; set; } = "An unexpected error occurred while processing your request.";

    private readonly ILogger<ErrorModel> _logger;

    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public void OnGet(int? statusCode)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        StatusCode = statusCode;

        switch (statusCode)
        {
            case 404:
                ErrorTitle = "Page Not Found";
                ErrorMessage = "Sorry, the page you're looking for doesn't exist or has been moved.";
                break;
            case 403:
                ErrorTitle = "Access Denied";
                ErrorMessage = "You don't have permission to access this page. Please contact an administrator.";
                break;
            case 401:
                ErrorTitle = "Unauthorized";
                ErrorMessage = "You need to be logged in to access this page.";
                break;
            case 500:
                ErrorTitle = "Server Error";
                ErrorMessage = "Something went wrong on our end. Please try again later.";
                break;
        }

        _logger.LogError("Error {StatusCode} occurred. RequestId: {RequestId}, Path: {Path}",
            statusCode, RequestId, HttpContext.Request.Path);
    }
}
