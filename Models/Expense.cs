using System.ComponentModel.DataAnnotations.Schema;

namespace TravelPlanner.Models
{
    /// <summary>
    /// Expense model for budget tracking.
    /// The model maps to the existing Expenses table schema.
    /// </summary>
    public class Expense
    {
        public int Id { get; set; }
        public int TripId { get; set; }

        [NotMapped]
        public string? Category { get; set; }

        public string? Description { get; set; }

        public decimal Amount { get; set; }
        [NotMapped]
        public string? Currency { get; set; } = "BDT";

        public DateTime ExpenseDate { get; set; }

        [NotMapped]
        public string? PaymentMethod { get; set; }

        [NotMapped]
        public bool IsPaid { get; set; } = true;

        [NotMapped]
        public string? Notes { get; set; }

        [NotMapped]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Trip? Trip { get; set; }
    }
}
