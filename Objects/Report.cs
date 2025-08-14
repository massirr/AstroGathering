using System;

namespace AstroGathering.Objects
{
    public class Report
    {
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public int PhotoId { get; set; }
        public string Reason { get; set; }
        public DateTime DateReported { get; set; }
        public string ReportStatus { get; set; } = "Pending";

        // Methods from UML diagram
        public bool SubmitReport()
        {
            DateReported = DateTime.Now;
            ReportStatus = "Pending";
            return true;
        }

        public bool DismissReport()
        {
            ReportStatus = "Dismissed";
            return true;
        }
    }
}
