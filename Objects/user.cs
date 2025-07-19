using System;

namespace AstroGathering.Objects
{
    public class User
    {
        public int Id { get; set; }
        public string? GoogleId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}