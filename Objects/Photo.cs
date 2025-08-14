using System;
using System.Collections.Generic;

namespace AstroGathering.Objects
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public int UserId { get; set; }
        public string ImageUrl { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public DateTime? DateTaken { get; set; }
        public DateTime TimeUploaded { get; set; }

        // Methods from UML diagram
        public bool UploadPhoto()
        {
            TimeUploaded = DateTime.Now;
            return true;
        }

        public bool DeletePhoto()
        {
            // Logic to delete photo
            return true;
        }

        public bool UpdateDescription(string description)
        {
            Description = description;
            return true;
        }

        public List<string> GetPhotoTags()
        {
            // This would be populated by the Data class
            return new List<string>();
        }

        public int GetLikes()
        {
            // This would be calculated by the Data class
            return 0;
        }
    }
}
