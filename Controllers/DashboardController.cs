using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TravelPlanner.Data;
using TravelPlanner.Models;
using TravelPlanner.Models.ViewModels;
using TravelPlanner.Repositories;

namespace TravelPlanner.Controllers
{
    /// <summary>
    /// Dashboard Controller for User Dashboard
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITripRepository _tripRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ApplicationDbContext _dbContext;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            ITripRepository tripRepository,
            IExpenseRepository expenseRepository,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var trips = await _tripRepository.GetUserTripsAsync(user.Id);
            var today = DateTime.Today;

            var activeTrips = trips
                .Where(t =>
                    (t.Status == "Ongoing" || t.Status == "Planned") &&
                    t.StartDate.Date <= today &&
                    t.EndDate.Date >= today)
                .OrderBy(t => t.StartDate)
                .ToList();

            var upcomingTrips = trips
                .Where(t => t.StartDate.Date > today)
                .OrderBy(t => t.StartDate)
                .ToList();

            var recentTrips = trips
                .Where(t => t.Status == "Completed" || t.EndDate.Date < today)
                .OrderByDescending(t => t.EndDate)
                .ToList();

            var tripReviews = await _dbContext.Reviews
                .Where(review => review.UserId == user.Id && review.TripId.HasValue)
                .ToDictionaryAsync(review => review.TripId!.Value);

            var viewModel = new DashboardViewModel
            {
                User = user,
                UpcomingTrips = upcomingTrips,
                RecentTrips = recentTrips,
                TotalTrips = trips.Count(),
                ActiveTrips = activeTrips.Count,
                TotalBudget = trips.Sum(t => t.BudgetLimit),
                TotalSpent = trips.Sum(t => t.TotalSpent),
                TripReviews = tripReviews
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTripReview(TripReviewInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please choose a rating from 1 to 5 stars.";
                return RedirectToAction(nameof(Index));
            }

            var trip = await _dbContext.Trips
                .SingleOrDefaultAsync(t => t.Id == model.TripId && t.UserId == user.Id);

            if (trip == null || (trip.Status != "Completed" && trip.Status != "Ended"))
            {
                TempData["Error"] = "Reviews are only available for completed trips.";
                return RedirectToAction(nameof(Index));
            }

            var alreadyReviewed = await _dbContext.Reviews
                .AnyAsync(review => review.TripId == trip.Id && review.UserId == user.Id);

            if (alreadyReviewed)
            {
                TempData["Message"] = "You have already reviewed this trip.";
                return RedirectToAction(nameof(Index));
            }

            _dbContext.Reviews.Add(new Review
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

            await _dbContext.SaveChangesAsync();
            TempData["Message"] = "Thanks for reviewing your trip!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;
            user.State = model.State;
            user.Country = model.Country;
            user.PostalCode = model.PostalCode;
            user.Bio = model.Bio;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                TempData["Message"] = "Profile updated successfully!";
            else
                TempData["Error"] = "Failed to update profile.";

            return RedirectToAction("Profile");
        }
    }

    public class TripReviewInputModel
    {
        public int TripId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(2000)]
        public string? Content { get; set; }
    }
}
