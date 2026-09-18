using System.ComponentModel.DataAnnotations.Schema;

namespace TravelPlanner.Models
{
    /// <summary>
    /// Destination model for travel locations
    /// </summary>
    public class Destination
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }

        [NotMapped]
        public string? State { get; set; }

        [NotMapped]
        public string? City { get; set; }

        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        [NotMapped]
        public decimal Latitude { get; set; }

        [NotMapped]
        public decimal Longitude { get; set; }

        [NotMapped]
        public string? WeatherType { get; set; }

        [Column("BestTimeToVisit")]
        public string? BestTimeToVisit { get; set; } = string.Empty;

        public string? Category { get; set; } // Beach, Mountain, City, Historical
        public decimal AverageRating { get; set; } = 0;

        [NotMapped]
        public int ReviewCount { get; set; } = 0;

        public bool IsPopular { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Trip>? Trips { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
        public virtual ICollection<Attraction>? Attractions { get; set; }
        public virtual ICollection<DestinationImage>? Images { get; set; }
        public virtual ICollection<Favorite>? Favorites { get; set; }
    }
}
