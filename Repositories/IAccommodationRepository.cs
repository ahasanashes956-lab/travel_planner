using TravelPlanner.Models;

namespace TravelPlanner.Repositories
{
    /// <summary>
    /// Accommodation Repository Interface
    /// </summary>
    public interface IAccommodationRepository
    {
        Task<IEnumerable<Accommodation>> GetAllAccommodationsAsync();
        Task<IEnumerable<Accommodation>> GetTripAccommodationsAsync(int tripId);
        Task<Accommodation> GetAccommodationByIdAsync(int id);
        Task<Accommodation> CreateAccommodationAsync(Accommodation accommodation);
        Task<Accommodation> UpdateAccommodationAsync(Accommodation accommodation);
        Task<bool> DeleteAccommodationAsync(int id);
    }
}
