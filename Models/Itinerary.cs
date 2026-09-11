namespace TravelPlanner.Models
{
    /// <summary>
    /// Itinerary model for day-wise schedule
    /// </summary>
    public class Itinerary
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int DayNumber { get; set; }
        public DateTime Date { get; set; }
        public string? Title { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Trip? Trip { get; set; }
        public virtual ICollection<Activity>? Activities { get; set; }
    }
}
