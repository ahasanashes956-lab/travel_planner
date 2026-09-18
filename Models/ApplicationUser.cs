using Microsoft.AspNetCore.Identity;

namespace TravelPlanner.Models
{
    /// <summary>
    /// Application User model with Identity
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumberCountryCode { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? Bio { get; set; }
        public string? Preferences { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        public virtual ICollection<Trip>? Trips { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
        public virtual ICollection<Favorite>? Favorites { get; set; }
    }
}
