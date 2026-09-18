using TravelPlanner.Models;

namespace TravelPlanner.Repositories
{
    /// <summary>
    /// Expense Repository Interface
    /// </summary>
    public interface IExpenseRepository
    {
        Task<IEnumerable<Expense>> GetAllExpensesAsync();
        Task<IEnumerable<Expense>> GetTripExpensesAsync(int tripId);
        Task<Expense> GetExpenseByIdAsync(int id);
        Task<decimal> GetTotalExpensesByCategoryAsync(int tripId, string category);
        Task<Expense> CreateExpenseAsync(Expense expense);
        Task<Expense> UpdateExpenseAsync(Expense expense);
        Task<bool> DeleteExpenseAsync(int id);
    }
}
