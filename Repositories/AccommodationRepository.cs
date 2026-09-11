using Microsoft.EntityFrameworkCore;
using TravelPlanner.Data;
using TravelPlanner.Models;

namespace TravelPlanner.Repositories
{
    /// <summary>
    /// Accommodation Repository Implementation
    /// </summary>
    public class AccommodationRepository : IAccommodationRepository
    {
        private readonly ApplicationDbContext _context;

        public AccommodationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Accommodation>> GetAllAccommodationsAsync()
        {
            return await _context.Accommodations.ToListAsync();
        }

        public async Task<IEnumerable<Accommodation>> GetTripAccommodationsAsync(int tripId)
        {
            return await _context.Accommodations
                .Where(a => a.TripId == tripId)
                .OrderBy(a => a.CheckInDate)
                .ToListAsync();
        }

        public async Task<Accommodation> GetAccommodationByIdAsync(int id)
        {
            return await _context.Accommodations.FindAsync(id);
        }

        public async Task<Accommodation> CreateAccommodationAsync(Accommodation accommodation)
        {
            _context.Accommodations.Add(accommodation);
            await _context.SaveChangesAsync();
            return accommodation;
        }

        public async Task<Accommodation> UpdateAccommodationAsync(Accommodation accommodation)
        {
            _context.Accommodations.Update(accommodation);
            await _context.SaveChangesAsync();
            return accommodation;
        }

        public async Task<bool> DeleteAccommodationAsync(int id)
        {
            var accommodation = await _context.Accommodations.FindAsync(id);
            if (accommodation == null) return false;

            _context.Accommodations.Remove(accommodation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
