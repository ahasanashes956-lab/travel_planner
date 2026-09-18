namespace TravelPlanner.Models
{
    /// <summary>
    /// Packing Item model for packing checklist
    /// </summary>
    public class PackingItem
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public string? ItemName { get; set; }
        public string? Category { get; set; } // Clothing, Toiletries, Documents, Electronics, Other
        public bool IsCompleted { get; set; } = false;
        public int Quantity { get; set; } = 1;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Trip? Trip { get; set; }
    }
}
