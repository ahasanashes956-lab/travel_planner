namespace TravelPlanner.Models
{
    /// <summary>
    /// Attraction model for tourist attractions
    /// </summary>
    public class Attraction
    {
        public int Id { get; set; }
        public int DestinationId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Category { get; set; } // Museum, Park, Temple, Beach
        public string? EntryFee { get; set; }
        public string? OpeningHours { get; set; }
        public decimal Rating { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Destination? Destination { get; set; }
    }
}
