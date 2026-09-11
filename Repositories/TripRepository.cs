using Microsoft.EntityFrameworkCore;
using TravelPlanner.Data;
using TravelPlanner.Models;

namespace TravelPlanner.Repositories
{
    /// <summary>
    /// Trip Repository Implementation
    /// </summary>
    public class TripRepository : ITripRepository
    {
        private readonly ApplicationDbContext _context;

        public TripRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Trip>> GetAllTripsAsync()
        {
            return await _context.Trips.Include(t => t.User).Include(t => t.Destination).ToListAsync();
        }

        public async Task<IEnumerable<Trip>> GetUserTripsAsync(string userId)
        {
            return await _context.Trips
                .Where(t => t.UserId == userId)
                .Include(t => t.Destination)
                .Select(t => new Trip
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Title = t.Title,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    DestinationId = t.DestinationId,
                    TripType = t.TripType,
                    Status = t.Status,
                    NumberOfTravelers = t.NumberOfTravelers,
                    BudgetLimit = t.BudgetLimit,
                    TotalSpent = _context.Expenses
                        .Where(expense => expense.TripId == t.Id)
                        .Sum(expense => (decimal?)expense.Amount) ?? 0,
                    CreatedAt = t.CreatedAt,
                    Destination = t.Destination == null ? null : new Destination
                    {
                        Id = t.Destination.Id,
                        Name = t.Destination.Name,
                        Country = t.Destination.Country
                    }
                })
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Trip> GetTripByIdAsync(int id)
        {
            var trip = await _context.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null)
                return null;

            trip.Destination = await _context.Destinations
                .AsNoTracking()
                .Where(destination => destination.Id == trip.DestinationId)
                .Select(destination => new Destination
                {
                    Id = destination.Id,
                    Name = destination.Name,
                    Country = destination.Country,
                    Description = destination.Description,
                    Category = destination.Category,
                    AverageRating = destination.AverageRating,
                    BestTimeToVisit = destination.BestTimeToVisit,
                    CreatedAt = destination.CreatedAt
                })
                .FirstOrDefaultAsync();

            trip.Expenses = await _context.Expenses
                .AsNoTracking()
                .Where(expense => expense.TripId == id)
                .OrderByDescending(expense => expense.ExpenseDate)
                .ToListAsync();

            trip.TotalSpent = trip.Expenses.Sum(expense => expense.Amount);
            trip.Itineraries = new List<Itinerary>();
            trip.Accommodations = new List<Accommodation>();
            trip.Transportations = new List<Transportation>();
            trip.PackingItems = new List<PackingItem>();

            return trip;
        }

        public async Task<Trip> CreateTripAsync(Trip trip)
        {
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            return trip;
        }

        public async Task<Trip> UpdateTripAsync(Trip trip)
        {
            trip.UpdatedAt = DateTime.UtcNow;
            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();
            return trip;
        }

        public async Task<bool> DeleteTripAsync(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return false;

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTripAsync(int id, string userId, bool isAdmin)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(item => item.Id == id && (isAdmin || item.UserId == userId));
            if (trip == null) return false;

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalExpensesAsync(int tripId)
        {
            return await _context.Expenses
                .Where(e => e.TripId == tripId)
                .SumAsync(e => e.Amount);
        }

        public async Task<int> GetTripCountAsync()
        {
            return await _context.Trips.CountAsync();
        }
    }
}
