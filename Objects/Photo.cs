using System;

namespace AstroGathering.Objects
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public int UserId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? EventName { get; set; }  // Store event name directly
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Description { get; set; }
        public DateTime? DateTaken { get; set; }
        public DateTime TimeUploaded { get; set; }
    }
}
