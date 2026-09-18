namespace TravelPlanner.Models
{
    /// <summary>
    /// Favorite model for saving destinations and hotels
    /// </summary>
    public class Favorite
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int DestinationId { get; set; }
        public string? Type { get; set; } // Destination, Hotel, Restaurant
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ApplicationUser? User { get; set; }
        public virtual Destination? Destination { get; set; }
    }
}
