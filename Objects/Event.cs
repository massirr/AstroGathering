using System;

namespace AstroGathering.Objects
{
    public class Event
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
