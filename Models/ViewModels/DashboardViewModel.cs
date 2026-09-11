using TravelPlanner.Models;

namespace TravelPlanner.Models.ViewModels
{
    public class DashboardViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Trip> Trips { get; set; }
        public List<Favorite> Favorites { get; set; }
        public int TotalTrips { get; set; }
        public int TotalDestinations { get; set; }
        public decimal TotalExpenses { get; set; }
        public int ActiveTrips { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public List<Trip> UpcomingTrips { get; set; }
        public List<Trip> RecentTrips { get; set; }
        public Dictionary<int, Review> TripReviews { get; set; } = new();
    }
}
