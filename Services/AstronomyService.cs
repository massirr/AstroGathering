using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Linq;

namespace AstroGathering.Services
{
    public class AstronomyService
    {
        private  string _accessKey;
        private string _expires;
        private string _signature;
        private  DateTime _serviceExpiryDate = new DateTime(2025, 8, 30, 23, 59, 59); // Stop on August 30th

        public AstronomyService(string apiKey, string expires, string signature)
        {
            _accessKey = apiKey;
            _expires = expires;
            _signature = signature;
            
            Console.WriteLine($"Service initialized. Will stop working after {_serviceExpiryDate:yyyy-MM-dd}");
        }

        public async Task<List<AstronomicalEvent>> GetEventsForDateAsync(DateTime date, string location = "oslo")
        {
            try
            {
                // Check if service has expired
                if (DateTime.Now > _serviceExpiryDate)
                {
                    Console.WriteLine($"Service has expired. No longer providing astronomy data after {_serviceExpiryDate:yyyy-MM-dd}");
                    return new List<AstronomicalEvent>();
                }

                // Check if current signature has expired
                if (IsSignatureExpired())
                {
                    Console.WriteLine("⚠️  WARNING: Current signature has expired!");
                    Console.WriteLine("   Please update the signature in the .env file to continue using the API.");
                    Console.WriteLine($"   Current expires: {_expires}");
                    Console.WriteLine($"   Current time: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
                    return new List<AstronomicalEvent>();
                }

                Console.WriteLine($"Fetching astronomy data for {date:yyyy-MM-dd} at {location}");
                
                // Get real API data only
                var realEvents = await GetRealApiEventsAsync(date, location);
                Console.WriteLine($"Found {realEvents.Count} real astronomical events");
                return realEvents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching astronomy data: {ex.Message}");
                return new List<AstronomicalEvent>();
            }
        }

        private bool IsSignatureExpired()
        {
            try
            {
                if (DateTime.TryParse(_expires, out var expiryTime))
                {
                    // Add a 5-minute buffer to avoid edge cases
                    return DateTime.UtcNow >= expiryTime.AddMinutes(-5);
                }
                return true; // If we can't parse the expiry time, assume it's expired
            }
            catch
            {
                return true;
            }
        }

        private async Task<List<AstronomicalEvent>> GetRealApiEventsAsync(DateTime date, string location)
        {
            try
            {
                using var httpClient = new HttpClient();
                
                // Check if required parameters are present
                if (string.IsNullOrEmpty(_accessKey) || string.IsNullOrEmpty(_expires) || string.IsNullOrEmpty(_signature))
                {
                    Console.WriteLine("ERROR: Missing required authentication parameters");
                    return new List<AstronomicalEvent>();
                }
                
                // Build the API URL using the pattern from your example
                var baseUrl = "https://api.xmltime.com/astronomy";
                var placeId = $"norway/{location}"; // Following your example format
                var startDate = date.ToString("yyyy-MM-dd");
                
                // URL encode the expires and signature parameters
                var encodedExpires = Uri.EscapeDataString(_expires);
                var encodedSignature = Uri.EscapeDataString(_signature);
                
                // Build the complete URL with all required authentication parameters
                var url = $"{baseUrl}?accesskey={_accessKey}&expires={encodedExpires}&signature={encodedSignature}&version=3&prettyprint=1&object=sun,moon&placeid={placeId}&startdt={startDate}";
                
                Console.WriteLine($"Calling API: {url.Replace(_accessKey, "***").Replace(_signature, "***")}"); // Hide sensitive data in logs
                
                var response = await httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API returned status code: {response.StatusCode}");
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("⚠️  API authentication failed. The signature may have expired.");
                        Console.WriteLine("   Please generate a new signature and update the .env file.");
                    }
                    return new List<AstronomicalEvent>();
                }
                
                var jsonContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response received, parsing...");
                
                return ParseApiResponse(jsonContent, date);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling real API: {ex.Message}");
                return new List<AstronomicalEvent>();
            }
        }

        private List<AstronomicalEvent> ParseApiResponse(string jsonContent, DateTime date)
        {
            var events = new List<AstronomicalEvent>();
            
            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;
                
                if (root.TryGetProperty("locations", out var locations) && locations.GetArrayLength() > 0)
                {
                    var location = locations[0];
                    if (location.TryGetProperty("astronomy", out var astronomy) &&
                        astronomy.TryGetProperty("objects", out var objects))
                    {
                        foreach (var obj in objects.EnumerateArray())
                        {
                            if (obj.TryGetProperty("name", out var objectName) &&
                                obj.TryGetProperty("days", out var days))
                            {
                                foreach (var day in days.EnumerateArray())
                                {
                                    if (day.TryGetProperty("events", out var dayEvents))
                                    {
                                        foreach (var eventItem in dayEvents.EnumerateArray())
                                        {
                                            if (eventItem.TryGetProperty("type", out var type) &&
                                                eventItem.TryGetProperty("hour", out var hour) &&
                                                eventItem.TryGetProperty("min", out var min))
                                            {
                                                var eventType = type.GetString();
                                                var eventHour = hour.GetInt32();
                                                var eventMin = min.GetInt32();
                                                var eventTime = $"{eventHour:D2}:{eventMin:D2}";
                                                
                                                var objectNameStr = objectName.GetString() ?? "unknown";
                                                var typeStr = eventType ?? "event";
                                                
                                                string description = objectNameStr.ToLower() switch
                                                {
                                                    "sun" => typeStr == "rise" ? "🌅 Sunrise" : "🌇 Sunset",
                                                    "moon" => typeStr == "rise" ? "🌙 Moonrise" : "🌙 Moonset",
                                                    _ => $"{objectNameStr} {typeStr}"
                                                };
                                                
                                                events.Add(new AstronomicalEvent
                                                {
                                                    Date = date,
                                                    EventType = ToTitleCase($"{objectNameStr} {typeStr}"),
                                                    Description = description,
                                                    Time = eventTime
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing API response: {ex.Message}");
            }
            
            return events;
        }

        private string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            var words = input.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + 
                              (words[i].Length > 1 ? words[i].Substring(1).ToLower() : "");
                }
            }
            return string.Join(" ", words);
        }
    }

    public class AstronomicalEvent
    {
        public DateTime Date { get; set; }
        public string EventType { get; set; } = "";
        public string Description { get; set; } = "";
        public string Time { get; set; } = "";
    }
}
