using System;

namespace AstroGathering.Objects
{
    public class User
    {
        public int UserId { get; set; }
        public string GoogleId { get; set; }
        public string Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsAdmin { get; set; }
        
        // OAuth properties - used by DesktopOAuthService
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        
        // Computed property for backwards compatibility
        public string Name => $"{FirstName} {LastName}".Trim();
    }
}
