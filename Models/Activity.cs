namespace TravelPlanner.Models
{
    /// <summary>
    /// Activity model for itinerary schedule
    /// </summary>
    public class Activity
    {
        public int Id { get; set; }
        public int ItineraryId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public string? Category { get; set; } // Sightseeing, Food, Adventure, Shopping
        public string? Notes { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Itinerary? Itinerary { get; set; }
    }
}
