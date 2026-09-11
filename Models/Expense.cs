using System.ComponentModel.DataAnnotations.Schema;

namespace TravelPlanner.Models
{
    /// <summary>
    /// Expense model for budget tracking.
    /// The database schema currently uses older column names, so we map the
    /// model properties to that schema instead of assuming the newer fields exist.
    /// </summary>
    public class Expense
    {
        public int Id { get; set; }
        public int TripId { get; set; }

        [NotMapped]
        public string? Category { get; set; }

        [Column("Name")]
        public string? Description { get; set; }

        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "BDT";

        [Column("IncurredAt")]
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
