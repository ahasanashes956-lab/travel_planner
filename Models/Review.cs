namespace TravelPlanner.Models
{
    /// <summary>
    /// Review model for ratings and feedback
    /// </summary>
    public class Review
    {
        public int Id { get; set; }
        public int DestinationId { get; set; }
        public int? TripId { get; set; }
        public string? UserId { get; set; }
        public int Rating { get; set; } // 1-5
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int HelpfulCount { get; set; } = 0;
        public bool IsApproved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Destination? Destination { get; set; }
        public virtual Trip? Trip { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
