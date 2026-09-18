using TravelPlanner.Models;

namespace TravelPlanner.Models.ViewModels
{
    public class TripCreateViewModel
    {
        public Trip Trip { get; set; }
        public List<Destination> Destinations { get; set; }
        public List<Accommodation> Accommodations { get; set; }
        public List<Activity> Activities { get; set; }
        public List<Itinerary> Itineraries { get; set; }
    }
}
