using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public TripController(
            UserManager<ApplicationUser> userManager,
            ITripRepository tripRepository,
            IDestinationRepository destinationRepository,
            IExpenseRepository expenseRepository)
        {
            _userManager = userManager;
            _tripRepository = tripRepository;
            _destinationRepository = destinationRepository;
            _expenseRepository = expenseRepository;
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
                createdAt = trip.CreatedAt
            }));
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
                // HTML date inputs bind as DateTimeKind.Unspecified. Render
                // PostgreSQL stores these columns as timestamptz, which only
                // accepts UTC DateTime values through Npgsql.
                StartDate = DateTime.SpecifyKind(model.StartDate.Date, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(model.EndDate.Date, DateTimeKind.Utc),
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

    public class AddExpenseApiViewModel
    {
        public string? Name { get; set; }
        public decimal Amount { get; set; }
    }
}
