using System;

namespace AstroGathering.Objects
{
    public class Like
    {
        public int UserId { get; set; }
        public int PhotoId { get; set; }
        public DateTime LikedAt { get; set; }
    }
}
