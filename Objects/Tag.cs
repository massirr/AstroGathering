using System;
using System.Collections.Generic;

namespace AstroGathering.Objects
{
    public class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; }

        // Methods from UML diagram
        public bool AddTagToPhoto(int photoId)
        {
            // This would be handled by the DatabaseManager class
            return true;
        }

        public bool RemoveTagFromPhoto(int photoId)
        {
            // This would be handled by the DatabaseManager class
            return true;
        }

        public List<Photo> SearchByTag(string tagName)
        {
            // This would be populated by the DatabaseManager class
            return new List<Photo>();
        }
    }
}
