using TravelPlanner.Models;

namespace TravelPlanner.Repositories
{
    /// <summary>
    /// Trip Repository Interface
    /// </summary>
    public interface ITripRepository
    {
        Task<IEnumerable<Trip>> GetAllTripsAsync();
        Task<IEnumerable<Trip>> GetUserTripsAsync(string userId);
        Task<Trip> GetTripByIdAsync(int id);
        Task<Trip> CreateTripAsync(Trip trip);
        Task<Trip> UpdateTripAsync(Trip trip);
        Task<bool> DeleteTripAsync(int id);
        Task<bool> DeleteTripAsync(int id, string userId, bool isAdmin);
        Task<decimal> GetTotalExpensesAsync(int tripId);
        Task<int> GetTripCountAsync();
    }
}
