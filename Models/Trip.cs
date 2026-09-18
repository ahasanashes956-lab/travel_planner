using System.ComponentModel.DataAnnotations.Schema;

namespace TravelPlanner.Models
{
    /// <summary>
    /// Trip model for travel planning
    /// </summary>
    public class Trip
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Title { get; set; }
        [Column("Notes")]
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DestinationId { get; set; }
        public string? TripType { get; set; } // Solo, Family, Friends, Business
        public string? Status { get; set; } = "Planned"; // Planned, Ongoing, Completed
        [Column("TravelersCount")]
        public int NumberOfTravelers { get; set; }
        [Column("Budget")]
        public decimal BudgetLimit { get; set; }
        public decimal TotalSpent { get; set; } = 0;
        public string? CoverImageUrl { get; set; }
        public bool IsPublic { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ApplicationUser? User { get; set; }
        public virtual Destination? Destination { get; set; }
        public virtual ICollection<Itinerary>? Itineraries { get; set; }
        public virtual ICollection<Expense>? Expenses { get; set; }
        public virtual ICollection<Accommodation>? Accommodations { get; set; }
        public virtual ICollection<Transportation>? Transportations { get; set; }
        public virtual ICollection<PackingItem>? PackingItems { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
    }
}
