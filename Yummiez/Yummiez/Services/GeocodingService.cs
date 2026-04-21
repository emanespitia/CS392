using System.Globalization;
using System.Text.Json;

namespace Yummiez.Services
{
    public class GeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeocodingService> _logger;

        public GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(double lat, double lng)?> TryGeocodeAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return null;
            }

            address = address.Trim();
            const int maxAddressLength = 500;
            if (address.Length > maxAddressLength)
            {
                address = address[..maxAddressLength];
            }

            try
            {
                var encodedAddress = Uri.EscapeDataString(address);
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"search?format=jsonv2&limit=1&q={encodedAddress}");

                request.Headers.UserAgent.ParseAdd("Yummiez-CS392/1.0");

                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(stream);

                var first = document.RootElement.EnumerateArray().FirstOrDefault();
                if (first.ValueKind == JsonValueKind.Undefined)
                {
                    return null;
                }

                if (!first.TryGetProperty("lat", out var latElement) ||
                    !first.TryGetProperty("lon", out var lonElement))
                {
                    return null;
                }

                if (!double.TryParse(latElement.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) ||
                    !double.TryParse(lonElement.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
                {
                    return null;
                }

                return (lat, lng);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to geocode address: {Address}", address);
                return null;
            }
        }
    }
}
