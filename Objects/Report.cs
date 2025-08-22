using System;

namespace AstroGathering.Objects
{
    public class Report
    {
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public int? PhotoId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public string ReportStatus { get; set; } = "Pending";
    }
}
