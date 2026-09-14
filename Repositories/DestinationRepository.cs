using Microsoft.EntityFrameworkCore;
using TravelPlanner.Data;
using TravelPlanner.Models;

namespace TravelPlanner.Repositories
{
    /// <summary>
    /// Destination Repository Implementation
    /// </summary>
    public class DestinationRepository : IDestinationRepository
    {
        private readonly ApplicationDbContext _context;

        public DestinationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Destination>> GetAllDestinationsAsync()
        {
            return await _context.Destinations
                .Include(d => d.Attractions)
                .Include(d => d.Images)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

            public async Task<IEnumerable<Destination>> GetPublishedDestinationsAsync()
            {
                return await _context.Destinations
                .Where(d => d.IsPublished)
                .Include(d => d.Attractions)
                .Include(d => d.Images)
                .OrderBy(d => d.Name)
                .ToListAsync();
            }

        public async Task<Destination> GetDestinationByIdAsync(int id)
        {
            return await _context.Destinations
                .Include(d => d.Attractions)
                .Include(d => d.Images)
                .Include(d => d.Reviews)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

            public async Task<Destination?> GetPublishedDestinationByIdAsync(int id)
            {
                return await _context.Destinations
                .Where(d => d.Id == id && d.IsPublished)
                .Include(d => d.Attractions)
                .Include(d => d.Images)
                .Include(d => d.Reviews)
                .FirstOrDefaultAsync();
            }

        public async Task<Destination?> GetDestinationSummaryByIdAsync(int id)
        {
            return await _context.Destinations
                .Where(destination => destination.Id == id && destination.IsPublished)
                .Select(destination => new Destination
                {
                    Id = destination.Id,
                    Name = destination.Name,
                    Country = destination.Country
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Destination>> GetDestinationSummariesAsync()
        {
            return await _context.Destinations
                .Where(destination => destination.IsPublished)
                .Select(destination => new Destination
                {
                    Id = destination.Id,
                    Name = destination.Name,
                    Country = destination.Country
                })
                .OrderBy(destination => destination.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Destination>> SearchDestinationsAsync(string query)
        {
            return await _context.Destinations
                .Where(d => d.IsPublished && (EF.Functions.Like(d.Name, $"%{query}%") ||
                           EF.Functions.Like(d.Country, $"%{query}%") ||
                           EF.Functions.Like(d.Region, $"%{query}%")))
                .Include(d => d.Attractions)
                .ToListAsync();
        }

        public async Task<IEnumerable<Destination>> GetPopularDestinationsAsync()
        {
            return await _context.Destinations
                .Where(d => d.IsPublished && d.IsPopular)
                .OrderByDescending(d => d.AverageRating)
                .Take(10)
                .Include(d => d.Images)
                .ToListAsync();
        }

        public async Task<Destination> CreateDestinationAsync(Destination destination)
        {
            _context.Destinations.Add(destination);
            await _context.SaveChangesAsync();
            return destination;
        }

        public async Task<Destination> UpdateDestinationAsync(Destination destination)
        {
            _context.Destinations.Update(destination);
            await _context.SaveChangesAsync();
            return destination;
        }

        public async Task<bool> DeleteDestinationAsync(int id)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null) return false;

            _context.Destinations.Remove(destination);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
