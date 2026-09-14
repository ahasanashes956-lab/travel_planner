using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Models;
using TravelPlanner.Services;

namespace TravelPlanner.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IChatService _chatService;

        public ChatController(UserManager<ApplicationUser> userManager, IChatService chatService)
        {
            _userManager = userManager;
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Please enter a message." });

            if (request.Message.Length > 2000)
                return BadRequest(new { message = "Please keep your message under 2,000 characters." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Please log in first." });

            try
            {
                var reply = await _chatService.GetReplyAsync(user, request, cancellationToken);
                return Ok(new { message = reply, conversationId = request.ConversationId });
            }
            catch (InvalidOperationException exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return StatusCode(StatusCodes.Status408RequestTimeout, new { message = "The AI request timed out. Please try again." });
            }
        }
    }
}
