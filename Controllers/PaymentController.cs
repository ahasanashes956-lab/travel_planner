using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelPlanner.Data;
using TravelPlanner.Models;

namespace TravelPlanner.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("trip/{tripId:int}")]
        public async Task<IActionResult> GetTripPayment(int tripId)
        {
            var user = await _userManager.GetUserAsync(User);
            var trip = await GetUserTripAsync(tripId, user?.Id);
            if (trip == null) return NotFound(new { message = "Trip not found" });

            var payment = await _context.Payments
                .Where(item => item.TripId == tripId && item.UserId == user!.Id)
                .OrderByDescending(item => item.CreatedAt)
                .FirstOrDefaultAsync();

            return Ok(payment == null ? new { status = "Unpaid", amount = trip.BudgetLimit } : new
            {
                status = payment.Status,
                amount = payment.Amount,
                paymentMethod = payment.PaymentMethod,
                transactionId = payment.TransactionId,
                paidAt = payment.PaidAt
            });
        }

        [HttpPost("initiate")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            var trip = await GetUserTripAsync(request?.TripId ?? 0, user?.Id);
            if (trip == null) return NotFound(new { message = "Trip not found" });
            if (trip.BudgetLimit <= 0) return BadRequest(new { message = "This trip has no payable budget" });

            var existingPaid = await _context.Payments.AnyAsync(payment =>
                payment.TripId == trip.Id && payment.UserId == user!.Id && payment.Status == "Paid");
            if (existingPaid) return BadRequest(new { message = "This trip is already paid" });

            var payment = new Payment
            {
                UserId = user!.Id,
                TripId = trip.Id,
                TransactionId = $"MOCK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..27].ToUpperInvariant(),
                Amount = trip.BudgetLimit,
                PaymentMethod = string.IsNullOrWhiteSpace(request?.PaymentMethod) ? "Mock Card" : request.PaymentMethod.Trim(),
                Status = "Pending",
                GatewayResponse = "Mock payment created"
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return Ok(new { paymentId = payment.Id, transactionId = payment.TransactionId, amount = payment.Amount, status = payment.Status });
        }

        [HttpPost("initiate-expense")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> InitiateExpensePayment([FromBody] ExpensePaymentRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            var trip = await GetUserTripAsync(request?.TripId ?? 0, user?.Id);
            if (trip == null) return NotFound(new { message = "Trip not found" });
            if (request!.Amount <= 0) return BadRequest(new { message = "Expense amount must be greater than zero" });

            var payment = new Payment
            {
                UserId = user!.Id,
                TripId = trip.Id,
                TransactionId = $"MOCK-EXP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..27].ToUpperInvariant(),
                Amount = request.Amount,
                PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Mock Card" : request.PaymentMethod.Trim(),
                Status = "Pending",
                GatewayResponse = "Mock expense payment created"
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return Ok(new { paymentId = payment.Id, transactionId = payment.TransactionId, amount = payment.Amount, status = payment.Status });
        }

        [HttpPost("{paymentId:int}/mock-result")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ProcessMockResult(int paymentId, [FromBody] MockPaymentResultRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            var payment = await _context.Payments.FirstOrDefaultAsync(item => item.Id == paymentId && item.UserId == user!.Id);
            if (payment == null) return NotFound(new { message = "Payment not found" });
            if (payment.Status == "Paid") return Ok(new { message = "Payment already completed", status = payment.Status });

            var result = request?.Result?.Trim().ToLowerInvariant();
            if (result is not ("success" or "failed" or "cancelled"))
                return BadRequest(new { message = "Invalid mock payment result" });

            payment.Status = result switch
            {
                "success" => "Paid",
                "failed" => "Failed",
                _ => "Cancelled"
            };
            payment.GatewayResponse = $"Mock payment {payment.Status.ToLowerInvariant()}";
            payment.PaidAt = payment.Status == "Paid" ? DateTime.UtcNow : null;

            if (payment.Status == "Paid")
            {
                var trip = await _context.Trips.FirstAsync(trip => trip.Id == payment.TripId && trip.UserId == user!.Id);
                trip.Status = "Confirmed";
                trip.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Mock payment {payment.Status.ToLowerInvariant()}", status = payment.Status, transactionId = payment.TransactionId });
        }

        private Task<Trip?> GetUserTripAsync(int tripId, string? userId)
        {
            return _context.Trips.FirstOrDefaultAsync(trip => trip.Id == tripId && trip.UserId == userId);
        }

        public sealed class InitiatePaymentRequest
        {
            public int TripId { get; set; }
            public string? PaymentMethod { get; set; }
        }

        public sealed class MockPaymentResultRequest
        {
            public string? Result { get; set; }
        }

        public sealed class ExpensePaymentRequest
        {
            public int TripId { get; set; }
            public decimal Amount { get; set; }
            public string? PaymentMethod { get; set; }
        }
    }
}