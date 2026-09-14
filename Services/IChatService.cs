using TravelPlanner.Models;

namespace TravelPlanner.Services
{
    public interface IChatService
    {
        Task<string> GetReplyAsync(ApplicationUser user, ChatRequest request, CancellationToken cancellationToken = default);
    }
}
