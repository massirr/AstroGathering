using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AstroGathering.Objects;
using AstroGathering.Database;
using System.Linq;

namespace AstroGathering.Services
{
    public class NasaApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _nasaApiKey;
        private readonly string _nasaBaseUrl = "https://api.nasa.gov";
        private readonly DatabaseOut _databaseOut;
        private readonly DatabaseIn _databaseIn;

        public NasaApiService()
        {
            _httpClient = new HttpClient();
            _nasaApiKey = ConfigurationService.NasaApiKey;
            _databaseOut = new DatabaseOut();
            _databaseIn = new DatabaseIn();
            
            // Set user agent for NASA API
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AstroGathering/1.0");
        }

        public async Task<List<AstronomicalEvent>> GetAstronomyEventsAsync(DateTime date)
        {
            var events = new List<AstronomicalEvent>();

            try
            {
                // Get Astronomy Picture of the Day
                var apodEvent = await GetAstronomyPictureOfDayAsync(date);
                if (apodEvent != null) events.Add(apodEvent);

                // Get Near Earth Objects for this date
                var neoEvents = await GetNearEarthObjectsAsync(date);
                events.AddRange(neoEvents);

                return events;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching NASA astronomy data: {ex.Message}");
                return new List<AstronomicalEvent>();
            }
        }

        public async Task<Dictionary<DateTime, List<AstronomicalEvent>>> GetMonthlyEventsAsync(DateTime month)
        {
            var monthlyEvents = new Dictionary<DateTime, List<AstronomicalEvent>>();

            try
            {
                Console.WriteLine($"Loading events for {month:MMMM yyyy}");

                // First, check if we have events in the database for this month
                var cachedEvents = _databaseOut.GetAstronomicalEventsForMonth(month);
                
                if (cachedEvents.Any())
                {
                    Console.WriteLine($"Found {cachedEvents.Count} cached events in database");
                    
                    // Group cached events by date
                    foreach (var cachedEvent in cachedEvents)
                    {
                        var eventDate = cachedEvent.EventDate.Date; // Updated to use EventDate
                        if (!monthlyEvents.ContainsKey(eventDate))
                            monthlyEvents[eventDate] = new List<AstronomicalEvent>();
                        monthlyEvents[eventDate].Add(cachedEvent);
                    }
                    
                    return monthlyEvents;
                }

                Console.WriteLine("No cached events found, fetching from NASA APIs...");

                // Get the first and last day of the month
                var firstDay = new DateTime(month.Year, month.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddDays(-1);
                
                var allEventsToStore = new List<AstronomicalEvent>();

                // Get APOD events for the entire month in one call
                var apodEvents = await GetMonthlyAPODAsync(firstDay, lastDay);
                
                // Set source for APOD events
                foreach (var apodEvent in apodEvents)
                {
                    apodEvent.Source = "NASA_APOD";
                    var eventDate = apodEvent.EventDate.Date; // Updated to use EventDate
                    if (!monthlyEvents.ContainsKey(eventDate))
                        monthlyEvents[eventDate] = new List<AstronomicalEvent>();
                    monthlyEvents[eventDate].Add(apodEvent);
                }
                allEventsToStore.AddRange(apodEvents);

                // Get NEO events for the month (NASA allows up to 7 days per request)
                await GetMonthlyNEOAsync(firstDay, lastDay, monthlyEvents);
                
                // Get NEO events from monthlyEvents for storing
                var neoEvents = monthlyEvents.Values
                    .SelectMany(events => events)
                    .Where(e => e.Type == "Near Earth Object")
                    .ToList();
                
                // Set source for NEO events
                foreach (var neoEvent in neoEvents)
                {
                    neoEvent.Source = "NASA_NEO";
                }
                allEventsToStore.AddRange(neoEvents);

                // Add moon phases for each day
                for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
                {
                    var moonPhases = await GetMoonPhaseAsync(date);
                    if (moonPhases.Any())
                    {
                        foreach (var moonPhase in moonPhases)
                        {
                            moonPhase.Source = "MOON_PHASE";
                        }
                        
                        if (!monthlyEvents.ContainsKey(date))
                            monthlyEvents[date] = new List<AstronomicalEvent>();
                        monthlyEvents[date].AddRange(moonPhases);
                        allEventsToStore.AddRange(moonPhases);
                    }
                }

                // Store all events in database for future use
                if (allEventsToStore.Any())
                {
                    Console.WriteLine($"Storing {allEventsToStore.Count} events in database");
                    var success = _databaseIn.InsertAstronomicalEvents(allEventsToStore);
                    Console.WriteLine($"Database storage {(success ? "successful" : "failed")}");
                }

                return monthlyEvents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching monthly NASA astronomy data: {ex.Message}");
                return new Dictionary<DateTime, List<AstronomicalEvent>>();
            }
        }

        // Method to clear cached data for a specific month to force fresh API calls
        public void ClearCachedDataForMonth(DateTime month)
        {
            try
            {
                Console.WriteLine($"Clearing cached data for {month:MMMM yyyy} to get fresh data with image URLs...");
                var success = _databaseIn.ClearAstronomicalEventsForMonth(month);
                Console.WriteLine($"Cache clearing {(success ? "successful" : "failed")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cached data: {ex.Message}");
            }
        }

        private async Task<AstronomicalEvent?> GetAstronomyPictureOfDayAsync(DateTime date)
        {
            try
            {
                var dateStr = date.ToString("yyyy-MM-dd");
                var url = $"{_nasaBaseUrl}/planetary/apod?api_key={_nasaApiKey}&date={dateStr}";
                
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apod = JObject.Parse(content);

                    var title = apod["title"]?.ToString() ?? "NASA APOD";
                    var explanation = apod["explanation"]?.ToString() ?? "";
                    var imageUrl = apod["url"]?.ToString() ?? "";
                    var hdImageUrl = apod["hdurl"]?.ToString() ?? "";
                    var mediaType = apod["media_type"]?.ToString() ?? "";
                    
                    // Only process if it's an image (skip videos and other media types)
                    if (mediaType != "image")
                    {
                        Console.WriteLine($"Skipping non-image APOD for {dateStr}: {mediaType}");
                        return null;
                    }
                    
                    var truncatedExplanation = explanation.Length > 200 ? 
                        explanation.Substring(0, 200) + "..." : explanation;

                    return new AstronomicalEvent
                    {
                        EventName = title, // Updated to use EventName
                        Type = "Astronomy Feature",
                        EventDate = date, // Updated to use EventDate
                        Description = truncatedExplanation,
                        ImageUrl = imageUrl,
                        HdImageUrl = hdImageUrl,
                        Time = "All day"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching APOD: {ex.Message}");
            }
            
            return null;
        }

        private async Task<List<AstronomicalEvent>> GetMonthlyAPODAsync(DateTime startDate, DateTime endDate)
        {
            var events = new List<AstronomicalEvent>();
            
            try
            {
                // APOD only has data up to today, so limit the end date
                var today = DateTime.Today;
                var actualEndDate = endDate > today ? today : endDate;
                
                // Skip if start date is in the future
                if (startDate > today)
                {
                    Console.WriteLine($"Skipping APOD for future dates: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                    return events;
                }
                
                // For now, make individual calls to avoid API errors with date ranges
                Console.WriteLine($"Fetching APOD data individually from {startDate:yyyy-MM-dd} to {actualEndDate:yyyy-MM-dd}");
                
                for (var date = startDate; date <= actualEndDate; date = date.AddDays(1))
                {
                    var apodEvent = await GetAstronomyPictureOfDayAsync(date);
                    if (apodEvent != null) 
                    {
                        events.Add(apodEvent);
                    }
                    
                    // Small delay to respect API rate limits
                    await Task.Delay(100);
                }
                
                Console.WriteLine($"Retrieved {events.Count} APOD events");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching monthly APOD: {ex.Message}");
            }
            
            return events;
        }

        private async Task<List<AstronomicalEvent>> GetNearEarthObjectsAsync(DateTime date)
        {
            var events = new List<AstronomicalEvent>();
            
            try
            {
                var dateStr = date.ToString("yyyy-MM-dd");
                var url = $"{_nasaBaseUrl}/neo/rest/v1/feed?start_date={dateStr}&end_date={dateStr}&api_key={_nasaApiKey}";
                
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var neoData = JObject.Parse(content);

                    var nearEarthObjects = neoData["near_earth_objects"]?[dateStr] as JArray;
                    
                    if (nearEarthObjects != null)
                    {
                        var count = 0;
                        foreach (var neo in nearEarthObjects)
                        {
                            if (count >= 2) break; // Limit to 2 objects per day
                            
                            var name = neo["name"]?.ToString();
                            var diameter = neo["estimated_diameter"]?["meters"]?["estimated_diameter_max"]?.ToString();
                            var distance = neo["close_approach_data"]?[0]?["miss_distance"]?["kilometers"]?.ToString();
                            var isPotentiallyHazardous = neo["is_potentially_hazardous_asteroid"]?.Value<bool>() ?? false;

                            if (!string.IsNullOrEmpty(name))
                            {
                                var hazardText = isPotentiallyHazardous ? " ⚠️" : "";
                                var distanceKm = TryParseDouble(distance);
                                var diameterM = TryParseDouble(diameter);
                                
                                events.Add(new AstronomicalEvent
                                {
                                    EventName = $"Asteroid {name}{hazardText}", // Updated to use EventName
                                    Type = "Near Earth Object",
                                    EventDate = date, // Updated to use EventDate
                                    Description = $"Distance: {FormatDistance(distanceKm)} km, Size: ~{FormatSize(diameterM)} m",
                                    Time = "Close approach today"
                                });
                                count++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching NEO data: {ex.Message}");
            }
            
            return events;
        }

        private async Task GetMonthlyNEOAsync(DateTime startDate, DateTime endDate, Dictionary<DateTime, List<AstronomicalEvent>> monthlyEvents)
        {
            try
            {
                // NASA NEO API allows max 7 days per request, so we need to chunk the month
                var currentDate = startDate;
                
                while (currentDate <= endDate)
                {
                    var chunkEndDate = currentDate.AddDays(6);
                    if (chunkEndDate > endDate) chunkEndDate = endDate;
                    
                    var startStr = currentDate.ToString("yyyy-MM-dd");
                    var endStr = chunkEndDate.ToString("yyyy-MM-dd");
                    var url = $"{_nasaBaseUrl}/neo/rest/v1/feed?start_date={startStr}&end_date={endStr}&api_key={_nasaApiKey}";
                    
                    Console.WriteLine($"Fetching NEO data from {startStr} to {endStr}");
                    
                    var response = await _httpClient.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var neoData = JObject.Parse(content);
                        var nearEarthObjects = neoData["near_earth_objects"];

                        if (nearEarthObjects != null)
                        {
                            foreach (var dateProperty in nearEarthObjects.Children<JProperty>())
                            {
                                if (DateTime.TryParse(dateProperty.Name, out var neoDate))
                                {
                                    var neoArray = dateProperty.Value as JArray;
                                    if (neoArray != null)
                                    {
                                        var count = 0;
                                        foreach (var neo in neoArray)
                                        {
                                            if (count >= 2) break; // Limit to 2 objects per day
                                            
                                            var name = neo["name"]?.ToString();
                                            var diameter = neo["estimated_diameter"]?["meters"]?["estimated_diameter_max"]?.ToString();
                                            var distance = neo["close_approach_data"]?[0]?["miss_distance"]?["kilometers"]?.ToString();
                                            var isPotentiallyHazardous = neo["is_potentially_hazardous_asteroid"]?.Value<bool>() ?? false;

                                            if (!string.IsNullOrEmpty(name))
                                            {
                                                var hazardText = isPotentiallyHazardous ? " ⚠️" : "";
                                                var distanceKm = TryParseDouble(distance);
                                                var diameterM = TryParseDouble(diameter);
                                                
                                                if (!monthlyEvents.ContainsKey(neoDate))
                                                    monthlyEvents[neoDate] = new List<AstronomicalEvent>();
                                                
                                                monthlyEvents[neoDate].Add(new AstronomicalEvent
                                                {
                                                    EventName = $"Asteroid {name}{hazardText}", // Updated to use EventName
                                                    Type = "Near Earth Object",
                                                    EventDate = neoDate, // Updated to use EventDate
                                                    Description = $"Distance: {FormatDistance(distanceKm)} km, Size: ~{FormatSize(diameterM)} m",
                                                    Time = "Close approach today"
                                                });
                                                count++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"NEO API error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                    
                    currentDate = chunkEndDate.AddDays(1);
                    
                    // Small delay to respect API rate limits
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching monthly NEO data: {ex.Message}");
            }
        }

        public async Task<List<AstronomicalEvent>> GetMoonPhaseAsync(DateTime date)
        {
            var events = new List<AstronomicalEvent>();
            
            try
            {
                // Simple moon phase calculation (approximate)
                var daysSinceNewMoon = GetDaysSinceNewMoon(date);
                var phase = GetMoonPhaseName(daysSinceNewMoon);
                
                if (!string.IsNullOrEmpty(phase))
                {
                    var emoji = GetMoonPhaseEmoji(phase);
                    events.Add(new AstronomicalEvent
                    {
                        EventName = $"{emoji} Moon Phase: {phase}", // Updated to use EventName
                        Type = "Moon Phase",
                        EventDate = date, // Updated to use EventDate
                        Description = $"The moon is in {phase} phase today",
                        Time = "Visible at night"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating moon phase: {ex.Message}");
            }
            
            return events;
        }

        // Helper methods
        private double GetDaysSinceNewMoon(DateTime date)
        {
            // Known new moon: January 6, 2000 at 18:14 UTC
            var knownNewMoon = new DateTime(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc);
            var lunarCycle = 29.53058867; // days
            
            var daysDifference = (date - knownNewMoon).TotalDays;
            return daysDifference % lunarCycle;
        }

        private string GetMoonPhaseName(double daysSinceNewMoon)
        {
            if (daysSinceNewMoon < 1) return "New Moon";
            if (daysSinceNewMoon < 7) return "Waxing Crescent";
            if (daysSinceNewMoon < 9) return "First Quarter";
            if (daysSinceNewMoon < 14) return "Waxing Gibbous";
            if (daysSinceNewMoon < 16) return "Full Moon";
            if (daysSinceNewMoon < 22) return "Waning Gibbous";
            if (daysSinceNewMoon < 24) return "Last Quarter";
            if (daysSinceNewMoon < 29) return "Waning Crescent";
            return "New Moon";
        }

        private string GetMoonPhaseEmoji(string phase)
        {
            return phase switch
            {
                "New Moon" => "🌑",
                "Waxing Crescent" => "🌒",
                "First Quarter" => "🌓",
                "Waxing Gibbous" => "🌔",
                "Full Moon" => "🌕",
                "Waning Gibbous" => "🌖",
                "Last Quarter" => "🌗",
                "Waning Crescent" => "🌘",
                _ => "🌙"
            };
        }

        private double TryParseDouble(string? value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            return double.TryParse(value, out var result) ? result : 0;
        }

        private string FormatDistance(double distance)
        {
            if (distance > 1000000) return $"{distance / 1000000:F1}M";
            if (distance > 1000) return $"{distance / 1000:F0}K";
            return $"{distance:F0}";
        }

        private string FormatSize(double size)
        {
            if (size > 1000) return $"{size / 1000:F1}km";
            return $"{size:F0}";
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
