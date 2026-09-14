using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TravelPlanner.Data;
using TravelPlanner.Models;

namespace TravelPlanner.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            ApplicationDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ChatService> logger)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetReplyAsync(ApplicationUser user, ChatRequest request, CancellationToken cancellationToken = default)
        {
            var endpoint = _configuration["AI:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
            var model = _configuration["AI:Model"] ?? "gpt-4o-mini";
            var apiKey = _configuration["AI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("AI chat is not configured yet. Add AI:ApiKey using .NET User Secrets or an environment variable.");

            var trips = await _dbContext.Trips
                .AsNoTracking()
                .Where(trip => trip.UserId == user.Id)
                .Include(trip => trip.Destination)
                .OrderByDescending(trip => trip.CreatedAt)
                .Take(10)
                .Select(trip => new
                {
                    trip.Title,
                    trip.Status,
                    trip.StartDate,
                    trip.EndDate,
                    trip.TripType,
                    trip.NumberOfTravelers,
                    trip.BudgetLimit,
                    Destination = trip.Destination == null ? null : trip.Destination.Name,
                    Country = trip.Destination == null ? null : trip.Destination.Country
                })
                .ToListAsync(cancellationToken);

            var destinations = await _dbContext.Destinations
                .AsNoTracking()
                .Where(destination => destination.IsPublished)
                .OrderByDescending(destination => destination.AverageRating)
                .Take(12)
                .Select(destination => new
                {
                    destination.Name,
                    destination.Country,
                    destination.Region,
                    destination.Description,
                    destination.Category,
                    destination.AverageRating
                })
                .ToListAsync(cancellationToken);

            var travelContext = JsonSerializer.Serialize(new { trips, destinations });
            var payload = new
            {
                model,
                temperature = 0.6,
                max_tokens = 700,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "You are Travel Planner BD's concise travel assistant. Give practical, safe, budget-aware travel advice. Use the supplied context when relevant, do not invent private user data, and say when information is uncertain. Format answers with short headings and bullet points. User travel context: " + travelContext
                    },
                    new { role = "user", content = request.Message!.Trim() }
                }
            };

            using var client = _httpClientFactory.CreateClient("TravelAi");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(payload)
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await client.SendAsync(httpRequest, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI provider returned {StatusCode}: {ResponseBody}", response.StatusCode, responseBody);
                var message = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => "The AI key was rejected. Create a new Groq key and update AI:ApiKey.",
                    System.Net.HttpStatusCode.NotFound => "The selected AI model or endpoint was not found. Check AI:Model and AI:Endpoint.",
                    (System.Net.HttpStatusCode)429 => "The free AI rate limit was reached. Please wait a moment and try again.",
                    _ => "The AI provider could not answer right now. Please try again shortly."
                };
                throw new InvalidOperationException(message);
            }

            using var document = JsonDocument.Parse(responseBody);
            var reply = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return string.IsNullOrWhiteSpace(reply)
                ? "I could not create an answer for that. Please try asking in a different way."
                : reply.Trim();
        }
    }
}
