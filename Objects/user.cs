using System;
using System.Collections.Generic;

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

        // Methods from your UML diagram
        public bool Login()
        {
            LastLogin = DateTime.Now;
            return true;
        }

        public void Logout()
        {
            // Handle logout logic
        }

        public bool UpdateProfile(string? firstName, string? lastName, string? profilePictureUrl)
        {
            FirstName = firstName;
            LastName = lastName;
            ProfilePictureUrl = profilePictureUrl;
            return true;
        }

        public bool IsAdminUser()
        {
            return IsAdmin;
        }
    }
}
