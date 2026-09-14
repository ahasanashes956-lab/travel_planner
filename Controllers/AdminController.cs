using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelPlanner.Data;
using TravelPlanner.Models;
using TravelPlanner.Repositories;

namespace TravelPlanner.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IDestinationRepository _destinationRepository;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IDestinationRepository destinationRepository)
        {
            _userManager = userManager;
            _context = context;
            _destinationRepository = destinationRepository;
        }

        private static string GetDestinationImage(Destination destination)
        {
            if (!string.IsNullOrWhiteSpace(destination.ImageUrl) &&
                !destination.ImageUrl.Contains("placeholder", StringComparison.OrdinalIgnoreCase))
            {
                return destination.ImageUrl;
            }

            return destination.Name?.ToLowerInvariant() switch
            {
                "paris" => "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?q=80&w=900&auto=format&fit=crop",
                "bali" => "https://images.unsplash.com/photo-1537996194471-e657df975ab4?q=80&w=900&auto=format&fit=crop",
                "tokyo" => "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?q=80&w=900&auto=format&fit=crop",
                "swiss alps" => "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?q=80&w=900&auto=format&fit=crop",
                "new york" => "https://images.unsplash.com/photo-1496588152823-86ff7695e68f?q=80&w=900&auto=format&fit=crop",
                _ => $"https://images.unsplash.com/photo-1500534623283-312aade485b7?q=80&w=900&auto=format&fit=crop"
            };
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users
                .Include(user => user.Trips)
                .OrderByDescending(user => user.CreatedAt)
                .ToListAsync();

            var result = new List<object>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                    continue;

                var status = !user.IsActive
                    ? "Rejected"
                    : user.IsVerified ? "Approved" : "Pending";

                result.Add(new
                {
                    id = user.Id,
                    email = user.Email,
                    name = $"{user.FirstName} {user.LastName}".Trim(),
                    roles,
                    status,
                    createdAt = user.CreatedAt,
                    lastLogin = user.LastLogin,
                    tripCount = user.Trips?.Count ?? 0
                });
            }

            return Ok(result);
        }

        [HttpPost("users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(string id, [FromBody] UserStatusRequest request)
        {
            if (request == null || !new[] { "pending", "approved", "rejected" }.Contains(request.Status.ToLowerInvariant()))
                return BadRequest(new { message = "Status must be pending, approved, or rejected." });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return BadRequest(new { message = "Admin accounts cannot be rejected." });

            switch (request.Status.ToLowerInvariant())
            {
                case "approved":
                    user.IsActive = true;
                    user.IsVerified = true;
                    break;
                case "rejected":
                    user.IsActive = false;
                    user.IsVerified = false;
                    break;
                default:
                    user.IsActive = true;
                    user.IsVerified = false;
                    break;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = string.Join(", ", result.Errors.Select(error => error.Description)) });

            return Ok(new { message = "User status updated." });
        }

        [HttpGet("destinations")]
        public async Task<IActionResult> GetDestinations()
        {
            var destinations = await _destinationRepository.GetAllDestinationsAsync();
            return Ok(destinations.Select(destination => new
            {
                id = destination.Id,
                name = destination.Name,
                country = destination.Country,
                region = destination.Region,
                description = destination.Description,
                imageUrl = GetDestinationImage(destination),
                bestTimeToVisit = destination.BestTimeToVisit,
                category = destination.Category,
                averageRating = destination.AverageRating,
                isPublished = destination.IsPublished,
                createdAt = destination.CreatedAt
            }));
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _context.Payments
                .Include(payment => payment.User)
                .Include(payment => payment.Trip)
                    .ThenInclude(trip => trip!.Destination)
                .OrderByDescending(payment => payment.CreatedAt)
                .ToListAsync();

            return Ok(payments.Select(payment => new
            {
                id = payment.Id,
                userName = $"{payment.User?.FirstName} {payment.User?.LastName}".Trim(),
                email = payment.User?.Email,
                tripTitle = payment.Trip?.Title,
                destination = payment.Trip?.Destination?.Name,
                amount = payment.Amount,
                currency = payment.Currency,
                paymentMethod = payment.PaymentMethod,
                status = payment.Status,
                transactionId = payment.TransactionId,
                createdAt = payment.CreatedAt,
                paidAt = payment.PaidAt
            }));
        }

        [HttpPost("destinations")]
        public async Task<IActionResult> CreateDestination([FromBody] CreateDestinationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Country) || string.IsNullOrWhiteSpace(request.Category))
                return BadRequest(new { message = "Name, country, and category are required." });

            var destination = new Destination
            {
                Name = request.Name.Trim(),
                Country = request.Country.Trim(),
                Region = request.Region?.Trim(),
                Description = request.Description?.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? "/images/travel-placeholder.svg" : request.ImageUrl.Trim(),
                BestTimeToVisit = request.BestTimeToVisit?.Trim(),
                Category = request.Category.Trim(),
                AverageRating = 0,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _destinationRepository.CreateDestinationAsync(destination);
            return Ok(new
            {
                message = "Destination created successfully.",
                destination = new
                {
                    id = created.Id,
                    name = created.Name,
                    country = created.Country,
                    region = created.Region,
                    description = created.Description,
                    imageUrl = created.ImageUrl,
                    bestTimeToVisit = created.BestTimeToVisit,
                    category = created.Category,
                    averageRating = created.AverageRating,
                    isPublished = created.IsPublished,
                    createdAt = created.CreatedAt
                }
            });
        }

        [HttpDelete("destinations/{id:int}")]
        public async Task<IActionResult> DeleteDestination(int id)
        {
            var deleted = await _destinationRepository.DeleteDestinationAsync(id);
            if (!deleted)
                return NotFound(new { message = "Destination not found." });

            return Ok(new { message = "Destination deleted successfully." });
        }

        [HttpPost("destinations/{id:int}/publish")]
        public async Task<IActionResult> SetDestinationPublished(int id, [FromBody] PublishDestinationRequest request)
        {
            var destination = await _destinationRepository.GetAllDestinationsAsync();
            var target = destination.FirstOrDefault(item => item.Id == id);
            if (target == null)
                return NotFound(new { message = "Destination not found." });

            target.IsPublished = request?.IsPublished ?? false;
            await _destinationRepository.UpdateDestinationAsync(target);
            return Ok(new { message = target.IsPublished ? "Destination published." : "Destination unpublished.", isPublished = target.IsPublished });
        }

        public sealed class UserStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }

        public sealed class CreateDestinationRequest
        {
            public string? Name { get; set; }
            public string? Country { get; set; }
            public string? Region { get; set; }
            public string? Description { get; set; }
            public string? ImageUrl { get; set; }
            public string? BestTimeToVisit { get; set; }
            public string? Category { get; set; }
        }

        public sealed class PublishDestinationRequest
        {
            public bool IsPublished { get; set; }
        }
    }
}
