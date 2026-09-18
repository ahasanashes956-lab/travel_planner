namespace TravelPlanner.Models
{
    /// <summary>
    /// Accommodation model for hotels and lodging
    /// </summary>
    public class Accommodation
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public string? HotelName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfRooms { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal TotalCost { get; set; }
        public string? RoomType { get; set; } // Single, Double, Suite
        public string? Status { get; set; } = "Booked"; // Booked, Confirmed, Cancelled
        public decimal HotelRating { get; set; } = 0;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Trip? Trip { get; set; }
    }
}
