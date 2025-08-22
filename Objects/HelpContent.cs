using System;

namespace AstroGathering.Objects
{
    public class HelpContent
    {
        public int SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }
}
