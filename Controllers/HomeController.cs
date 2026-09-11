using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Repositories;

namespace TravelPlanner.Controllers
{
    /// <summary>
    /// Home Controller
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IDestinationRepository _destinationRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IDestinationRepository destinationRepository, ILogger<HomeController> logger)
        {
            _destinationRepository = destinationRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var popularDestinations = await _destinationRepository.GetPopularDestinationsAsync();
            return View(popularDestinations);
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }
    }
}
