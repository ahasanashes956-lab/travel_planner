namespace TravelPlanner.Models
{
    /// <summary>
    /// DestinationImage model for image gallery
    /// </summary>
    public class DestinationImage
    {
        public int Id { get; set; }
        public int DestinationId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Destination? Destination { get; set; }
    }
}
