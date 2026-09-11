namespace TravelPlanner.Models
{
    /// <summary>
    /// Transportation model for flights, trains, buses
    /// </summary>
    public class Transportation
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public string? TransportType { get; set; } // Flight, Train, Bus, Car
        public string? From { get; set; }
        public string? To { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public string? Provider { get; set; } // Airline, Railway, Bus Company
        public string? TicketNumber { get; set; }
        public string? SeatNumber { get; set; }
        public decimal Cost { get; set; }
        public int Travelers { get; set; }
        public string? Status { get; set; } = "Booked"; // Booked, Confirmed, Completed
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Trip? Trip { get; set; }
    }
}
