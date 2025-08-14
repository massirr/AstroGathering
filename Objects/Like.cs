using System;
using System.Collections.Generic;

namespace AstroGathering.Objects
{
    public class Like
    {
        public int UserId { get; set; }
        public int PhotoId { get; set; }
        public DateTime LikedAt { get; set; }

        // Methods from UML diagram
        public bool AddLike(int userId, int photoId)
        {
            UserId = userId;
            PhotoId = photoId;
            LikedAt = DateTime.Now;
            return true;
        }

        public bool RemoveLike(int userId, int photoId)
        {
            // This would be handled by the DatabaseManager class
            return true;
        }

        public List<Photo> GetLikedPhotos(int userId)
        {
            // This would be populated by the DatabaseManager class
            return new List<Photo>();
        }
    }
}
