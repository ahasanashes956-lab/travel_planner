using TravelPlanner.Models;

namespace TravelPlanner.Repositories
{
    /// <summary>
    /// Destination Repository Interface
    /// </summary>
    public interface IDestinationRepository
    {
        Task<IEnumerable<Destination>> GetAllDestinationsAsync();
        Task<Destination> GetDestinationByIdAsync(int id);
        Task<Destination?> GetDestinationSummaryByIdAsync(int id);
        Task<IEnumerable<Destination>> GetDestinationSummariesAsync();
        Task<IEnumerable<Destination>> SearchDestinationsAsync(string query);
        Task<IEnumerable<Destination>> GetPopularDestinationsAsync();
        Task<Destination> CreateDestinationAsync(Destination destination);
        Task<Destination> UpdateDestinationAsync(Destination destination);
        Task<bool> DeleteDestinationAsync(int id);
    }
}
