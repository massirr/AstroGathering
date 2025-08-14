using System;
using System.Collections.Generic;

namespace AstroGathering.Objects
{
    public class Event
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string EventName { get; set; }
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // Methods from UML diagram
        public List<Event> GetUpcomingEvents()
        {
            // This would be populated by the Data class
            return new List<Event>();
        }

        public List<Event> GetEventsByDateRange(DateTime startDate, DateTime endDate)
        {
            // This would be populated by the Data class
            return new List<Event>();
        }
    }
}
