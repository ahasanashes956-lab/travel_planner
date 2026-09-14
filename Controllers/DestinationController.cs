using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Models;
using TravelPlanner.Repositories;

namespace TravelPlanner.Controllers
{
    /// <summary>
    /// Destination Controller for Destination Explorer
    /// </summary>
    public class DestinationController : Controller
    {
        private readonly IDestinationRepository _destinationRepository;

        private static string GetDestinationImage(Destination destination)
        {
            if (!string.IsNullOrWhiteSpace(destination.ImageUrl) &&
                !destination.ImageUrl.Contains("placeholder", StringComparison.OrdinalIgnoreCase))
            {
                return destination.ImageUrl;
            }

            return destination.Name.ToLowerInvariant() switch
            {
                "paris" => "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?q=80&w=900&auto=format&fit=crop",
                "bali" => "https://images.unsplash.com/photo-1537996194471-e657df975ab4?q=80&w=900&auto=format&fit=crop",
                "tokyo" => "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?q=80&w=900&auto=format&fit=crop",
                "swiss alps" => "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?q=80&w=900&auto=format&fit=crop",
                "new york" => "https://images.unsplash.com/photo-1496588152823-86ff7695e68f?q=80&w=900&auto=format&fit=crop",
                _ => "/images/travel-placeholder.svg"
            };
        }

        public DestinationController(IDestinationRepository destinationRepository)
        {
            _destinationRepository = destinationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var destinations = await _destinationRepository.GetPublishedDestinationsAsync();
            return View(destinations);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var destination = await _destinationRepository.GetPublishedDestinationByIdAsync(id);
            if (destination == null)
                return NotFound();

            return View(destination);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
                return RedirectToAction("Index");

            var results = await _destinationRepository.SearchDestinationsAsync(query);
            return View("Index", results);
        }

        [HttpGet("/api/destinations/catalog")]
        public async Task<IActionResult> Catalog()
        {
            var destinations = await _destinationRepository.GetPublishedDestinationsAsync();
            return Ok(destinations.Select(destination => new
            {
                id = destination.Id,
                name = destination.Name,
                country = destination.Country,
                city = destination.Region ?? destination.Country,
                description = destination.Description,
                image = GetDestinationImage(destination),
                rating = destination.AverageRating,
                reviews = destination.Reviews?.Count ?? 0,
                category = destination.Category,
                attractions = string.Join(", ", destination.Attractions?.Select(attraction => attraction.Name) ?? Enumerable.Empty<string>()),
                supported = true
            }));
        }
    }
}
