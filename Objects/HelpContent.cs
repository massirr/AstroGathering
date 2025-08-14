using System;
using System.Collections.Generic;

namespace AstroGathering.Objects
{
    public class HelpContent
    {
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime LastUpdated { get; set; }

        // Methods from UML diagram
        public List<HelpContent> GetAllSections()
        {
            // This would be populated by the DatabaseManager class
            return new List<HelpContent>();
        }

        public HelpContent SearchContentByKeyword(string keyword)
        {
            // This would be populated by the DatabaseManager class
            return new HelpContent();
        }

        public bool UpdateContent(string content)
        {
            Content = content;
            LastUpdated = DateTime.Now;
            return true;
        }

        public bool Publish()
        {
            LastUpdated = DateTime.Now;
            return true;
        }
    }
}
