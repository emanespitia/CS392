using System.Text.RegularExpressions;

namespace Yummiez.Helpers;

public static partial class InputValidation
{
    public const int MaxSearchQueryLength = 200;
    public const int MaxCategoryFilterLength = 50;
    public const int MaxIdentityUserIdLength = 450;

    /// <summary>
    /// ASP.NET Core Identity default stores user ids as GUID strings.
    /// </summary>
    public static bool IsValidIdentityUserId(string? userId) =>
        !string.IsNullOrWhiteSpace(userId)
        && userId.Length <= MaxIdentityUserIdLength
        && Guid.TryParse(userId, out _);

    public static bool IsValidPositiveOrderId(int orderId) => orderId > 0;

    public static bool IsValidLatitudeLongitude(double lat, double lng) =>
        double.IsFinite(lat) && double.IsFinite(lng)
        && lat is >= -90 and <= 90
        && lng is >= -180 and <= 180;

    /// <summary>
    /// Trims and caps length for search/query parameters. Returns null if empty after trim.
    /// </summary>
    public static string? NormalizeSearchQuery(string? value, int maxLength = MaxSearchQueryLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            trimmed = trimmed[..maxLength];
        }

        return trimmed.Length == 0 ? null : trimmed;
    }

    /// <summary>
    /// Category filter must be a short label (letters, numbers, spaces).
    /// </summary>
    public static string? NormalizeCategoryFilter(string? value, int maxLength = MaxCategoryFilterLength)
    {
        var q = NormalizeSearchQuery(value, maxLength);
        if (q == null)
        {
            return null;
        }

        return SafeCategoryLabelRegex().IsMatch(q) ? q : null;
    }

    [GeneratedRegex("^[a-zA-Z0-9][a-zA-Z0-9 \\-&.']{0,49}$", RegexOptions.CultureInvariant)]
    private static partial Regex SafeCategoryLabelRegex();
}
