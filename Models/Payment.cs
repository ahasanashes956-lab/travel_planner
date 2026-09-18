namespace TravelPlanner.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int TripId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BDT";
        public string PaymentMethod { get; set; } = "Mock Card";
        public string Status { get; set; } = "Pending";
        public string? GatewayResponse { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual Trip? Trip { get; set; }
    }
}