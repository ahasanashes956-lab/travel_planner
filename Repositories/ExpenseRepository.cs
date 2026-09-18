using Microsoft.EntityFrameworkCore;
using TravelPlanner.Data;
using TravelPlanner.Models;

namespace TravelPlanner.Repositories
{
    /// <summary>
    /// Expense Repository Implementation
    /// </summary>
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ApplicationDbContext _context;

        public ExpenseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Expense>> GetAllExpensesAsync()
        {
            return await _context.Expenses.ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetTripExpensesAsync(int tripId)
        {
            return await _context.Expenses
                .Where(e => e.TripId == tripId)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        public async Task<Expense> GetExpenseByIdAsync(int id)
        {
            return await _context.Expenses.FindAsync(id);
        }

        public async Task<decimal> GetTotalExpensesByCategoryAsync(int tripId, string category)
        {
            return await _context.Expenses
                .Where(e => e.TripId == tripId && e.Category == category)
                .SumAsync(e => e.Amount);
        }

        public async Task<Expense> CreateExpenseAsync(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<Expense> UpdateExpenseAsync(Expense expense)
        {
            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return false;

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
