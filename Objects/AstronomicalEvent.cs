using System;

namespace AstroGathering.Objects
{
    public class AstronomicalEvent
    {
        public int Id { get; set; } // Database ID
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty; // Standard resolution image
        public string HdImageUrl { get; set; } = string.Empty; // High definition image
        public string Time { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Source { get; set; } = string.Empty; // "NASA_APOD", "NASA_NEO", "MOON_PHASE"
    }
}