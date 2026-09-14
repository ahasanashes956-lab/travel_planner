using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelPlanner.Data;
using TravelPlanner.Models;
using TravelPlanner.Models.ViewModels;
using TravelPlanner.Repositories;

namespace TravelPlanner.Controllers
{
    /// <summary>
    /// Trip Controller for Trip Management
    /// </summary>
    public class TripController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITripRepository _tripRepository;
        private readonly IDestinationRepository _destinationRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ApplicationDbContext _context;

        public TripController(
            UserManager<ApplicationUser> userManager,
            ITripRepository tripRepository,
            IDestinationRepository destinationRepository,
            IExpenseRepository expenseRepository,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _tripRepository = tripRepository;
            _destinationRepository = destinationRepository;
            _expenseRepository = expenseRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var trips = await _tripRepository.GetUserTripsAsync(user.Id);
            return View(trips);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var destinations = await _destinationRepository.GetAllDestinationsAsync();
            var model = new TripCreateViewModel
            {
                Destinations = destinations.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trip trip)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (ModelState.IsValid)
            {
                trip.UserId = user.Id;
                trip.CreatedAt = DateTime.UtcNow;
                trip.UpdatedAt = DateTime.UtcNow;

                var result = await _tripRepository.CreateTripAsync(trip);
                TempData["Message"] = "Trip created successfully!";
                return RedirectToAction("Details", new { id = result.Id });
            }

            var destinations = await _destinationRepository.GetAllDestinationsAsync();
            var model = new TripCreateViewModel
            {
                Trip = trip,
                Destinations = destinations.ToList()
            };
            return View(model);
        }

        [Authorize]
        [HttpGet("/api/trips")]
        public async Task<IActionResult> GetTripsApi()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Please log in first" });

            var trips = await _tripRepository.GetUserTripsAsync(user.Id);
            var completedTrips = trips
                .Where(trip => trip.EndDate.Date < DateTime.Today &&
                    trip.Status != "Completed" && trip.Status != "Ended")
                .ToList();

            foreach (var trip in completedTrips)
            {
                trip.Status = "Completed";
                trip.UpdatedAt = DateTime.UtcNow;
            }

            if (completedTrips.Count > 0)
                await _context.SaveChangesAsync();

            var reviews = await _context.Reviews
                .Where(review => review.UserId == user.Id && review.TripId.HasValue)
                .ToDictionaryAsync(review => review.TripId!.Value);

            return Ok(trips.Select(trip => new
            {
                id = trip.Id,
                title = trip.Title,
                description = trip.Description,
                destination = trip.Destination == null ? null : new
                {
                    id = trip.Destination.Id,
                    name = trip.Destination.Name,
                    country = trip.Destination.Country,
                    imageUrl = trip.Destination.ImageUrl
                },
                coverImageUrl = trip.CoverImageUrl,
                tripType = trip.TripType,
                status = trip.Status,
                startDate = trip.StartDate,
                endDate = trip.EndDate,
                numberOfTravelers = trip.NumberOfTravelers,
                budgetLimit = trip.BudgetLimit,
                totalSpent = trip.TotalSpent,
                createdAt = trip.CreatedAt,
                review = reviews.TryGetValue(trip.Id, out var review) ? new
                {
                    rating = review.Rating,
                    content = review.Content
                } : null
            }));
        }

        [Authorize]
        [HttpPost("/api/trips/{tripId:int}/review")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SubmitTripReviewApi(int tripId, [FromBody] TripReviewApiViewModel model)
        {
            if (model == null || model.Rating < 1 || model.Rating > 5)
                return BadRequest(new { message = "Please choose a rating from 1 to 5 stars." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Please log in first" });

            var trip = await _context.Trips
                .SingleOrDefaultAsync(existingTrip => existingTrip.Id == tripId && existingTrip.UserId == user.Id);

            if (trip == null)
                return NotFound(new { message = "Trip not found" });

            if (trip.EndDate.Date < DateTime.Today && trip.Status != "Completed" && trip.Status != "Ended")
            {
                trip.Status = "Completed";
                trip.UpdatedAt = DateTime.UtcNow;
            }

            if (trip.Status != "Completed" && trip.Status != "Ended")
                return BadRequest(new { message = "Reviews are available after the trip is completed." });

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(review => review.TripId == tripId && review.UserId == user.Id);
            if (alreadyReviewed)
                return Conflict(new { message = "You have already reviewed this trip." });

            _context.Reviews.Add(new Review
            {
                TripId = trip.Id,
                DestinationId = trip.DestinationId,
                UserId = user.Id,
                Rating = model.Rating,
                Content = string.IsNullOrWhiteSpace(model.Content) ? null : model.Content.Trim(),
                Title = "Trip review",
                CreatedAt = DateTime.UtcNow,
                IsApproved = false
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Thanks for reviewing your trip!" });
        }

        [Authorize]
        [HttpGet("/api/destinations")]
        public async Task<IActionResult> GetDestinationSummariesApi()
        {
            var destinations = await _destinationRepository.GetDestinationSummariesAsync();
            return Ok(destinations.Select(destination => new
            {
                id = destination.Id,
                name = destination.Name,
                country = destination.Country
            }));
        }

        [Authorize]
        [HttpPost("/api/trips")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateTripApi([FromBody] CreateTripApiViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Title) ||
                model.DestinationId <= 0 || string.IsNullOrWhiteSpace(model.TripType))
                return BadRequest(new { message = "Title, destination, and trip type are required" });

            if (model.StartDate == default || model.EndDate == default)
                return BadRequest(new { message = "Start date and end date are required" });

            if (model.StartDate.Date < DateTime.Today || model.EndDate.Date < DateTime.Today)
                return BadRequest(new { message = "Trip dates must be today or later" });

            if (model.EndDate.Date < model.StartDate.Date)
                return BadRequest(new { message = "End date cannot be before the start date" });

            if (model.NumberOfTravelers < 1 || model.BudgetLimit < 0)
                return BadRequest(new { message = "Travelers and budget must be valid" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Please log in first" });

            var duplicateExists = (await _tripRepository.GetUserTripsAsync(user.Id)).Any(existing =>
                string.Equals(existing.Title?.Trim(), model.Title.Trim(), StringComparison.OrdinalIgnoreCase) &&
                existing.DestinationId == model.DestinationId &&
                existing.StartDate.Date == model.StartDate.Date &&
                existing.EndDate.Date == model.EndDate.Date);

            var destination = await _destinationRepository.GetDestinationSummaryByIdAsync(model.DestinationId);
            if (destination == null)
                return BadRequest(new { message = "Selected destination was not found" });

            var trip = new Trip
            {
                UserId = user.Id,
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                DestinationId = destination.Id,
                TripType = model.TripType.Trim(),
                StartDate = model.StartDate.Date,
                EndDate = model.EndDate.Date,
                NumberOfTravelers = model.NumberOfTravelers,
                BudgetLimit = model.BudgetLimit,
                Status = "Planned",
                TotalSpent = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdTrip = await _tripRepository.CreateTripAsync(trip);
            return Ok(new
            {
                message = "Trip created successfully",
                isDuplicate = duplicateExists,
                trip = new
                {
                    id = createdTrip.Id,
                    title = createdTrip.Title,
                    destination = destination.Name,
                    status = createdTrip.Status
                }
            });
        }

        [Authorize]
        [HttpPost("/api/trips/{tripId}/expenses")]
        [HttpPost("api/trips/{tripId}/expenses")]
        [HttpPost("/api/trips/{tripId:int}/expenses")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddExpenseApi(int tripId, [FromBody] AddExpenseApiViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name) || model.Amount <= 0)
                return BadRequest(new { message = "Expense name and amount are required" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Please log in first" });

            var trip = await _tripRepository.GetTripByIdAsync(tripId);
            if (trip == null)
                return NotFound(new { message = "Trip not found" });

            if (trip.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            var expensePayment = await _context.Payments.FirstOrDefaultAsync(payment =>
                payment.Id == model.PaymentId && payment.TripId == trip.Id && payment.UserId == user.Id &&
                payment.Status == "Paid" && Math.Abs(payment.Amount - model.Amount) < 0.01m);

            // Keep the expense flow resilient if the browser submits a stale payment id
            // after the mock payment has already completed.
            expensePayment ??= await _context.Payments
                .Where(payment => payment.TripId == trip.Id && payment.UserId == user.Id &&
                    payment.Status == "Paid" && Math.Abs(payment.Amount - model.Amount) < 0.01m)
                .OrderByDescending(payment => payment.PaidAt ?? payment.CreatedAt)
                .FirstOrDefaultAsync();

            if (expensePayment == null)
                return BadRequest(new { message = "Please complete a payment for the exact expense amount before saving it." });

            var expense = new Expense
            {
                TripId = trip.Id,
                Description = model.Name.Trim(),
                Category = "Other",
                Amount = model.Amount,
                Currency = "BDT",
                ExpenseDate = DateTime.UtcNow,
                IsPaid = true,
                Notes = "Added from My Trips",
                CreatedAt = DateTime.UtcNow
            };

            await _expenseRepository.CreateExpenseAsync(expense);

            trip.TotalSpent += expense.Amount;
            trip.UpdatedAt = DateTime.UtcNow;
            await _tripRepository.UpdateTripAsync(trip);

            return Ok(new
            {
                message = "Expense added successfully",
                expense = new
                {
                    id = expense.Id,
                    name = expense.Description,
                    amount = expense.Amount,
                    tripId = expense.TripId
                }
            });
        }

        [Authorize]
        [HttpDelete("/api/trips/{tripId:int}")]
        public async Task<IActionResult> DeleteTripApi(int tripId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Please log in first" });

            var deleted = await _tripRepository.DeleteTripAsync(tripId, user.Id, User.IsInRole("Admin"));
            if (!deleted)
                return NotFound(new { message = "Trip not found or you do not have permission to delete it" });

            return Ok(new { message = "Trip deleted successfully" });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var trip = await _tripRepository.GetTripByIdAsync(id);
            if (trip == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (trip.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            return View(trip);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var trip = await _tripRepository.GetTripByIdAsync(id);
            if (trip == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (trip.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            var destinations = await _destinationRepository.GetAllDestinationsAsync();
            var model = new TripCreateViewModel
            {
                Trip = trip,
                Destinations = destinations.ToList()
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trip trip)
        {
            if (id != trip.Id)
                return NotFound();

            var existingTrip = await _tripRepository.GetTripByIdAsync(id);
            if (existingTrip == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (existingTrip.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            if (ModelState.IsValid)
            {
                existingTrip.Title = trip.Title;
                existingTrip.Description = trip.Description;
                existingTrip.StartDate = trip.StartDate;
                existingTrip.EndDate = trip.EndDate;
                existingTrip.DestinationId = trip.DestinationId;
                existingTrip.TripType = trip.TripType;
                existingTrip.Status = trip.Status;
                existingTrip.NumberOfTravelers = trip.NumberOfTravelers;
                existingTrip.BudgetLimit = trip.BudgetLimit;

                await _tripRepository.UpdateTripAsync(existingTrip);
                TempData["Message"] = "Trip updated successfully!";
                return Redirect($"/trips.html?tripId={trip.Id}");
            }

            var destinations = await _destinationRepository.GetAllDestinationsAsync();
            var model = new TripCreateViewModel
            {
                Trip = trip,
                Destinations = destinations.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var trip = await _tripRepository.GetTripByIdAsync(id);
            if (trip == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (trip.UserId != user.Id)
                return Forbid();

            await _tripRepository.DeleteTripAsync(id);
            TempData["Message"] = "Trip deleted successfully!";
            return RedirectToAction("Index");
        }
    }

    public class CreateTripApiViewModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int DestinationId { get; set; }
        public string? TripType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfTravelers { get; set; }
        public decimal BudgetLimit { get; set; }
    }

    public class TripReviewApiViewModel
    {
        public int Rating { get; set; }
        public string? Content { get; set; }
    }

    public class AddExpenseApiViewModel
    {
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public int PaymentId { get; set; }
    }
}
