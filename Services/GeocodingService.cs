using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace AstroGathering.Services
{
    public class GeocodingService
    {
        private readonly HttpClient _httpClient;

        public GeocodingService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AstroGathering");
        }

        public async Task<(double? Latitude, double? Longitude, string? Error)> GetCoordinatesAsync(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return (null, null, "Location cannot be empty");
            }

            try
            {
                var encodedLocation = HttpUtility.UrlEncode(location);
                var url = $"https://nominatim.openstreetmap.org/search?q={encodedLocation}&format=json&limit=1";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<NominatimResult[]>(jsonString);

                if (results == null || results.Length == 0)
                {
                    return (null, null, "Location not found");
                }

                var result = results[0];
                if (double.TryParse(result.lat, out var latitude) && double.TryParse(result.lon, out var longitude))
                {
                    return (latitude, longitude, null);
                }

                return (null, null, "Invalid coordinates received");
            }
            catch (HttpRequestException ex)
            {
                return (null, null, $"Network error: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return (null, null, $"Invalid response format: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (null, null, $"Unexpected error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class NominatimResult
    {
        public string lat { get; set; } = string.Empty;
        public string lon { get; set; } = string.Empty;
        public string display_name { get; set; } = string.Empty;
    }
}
