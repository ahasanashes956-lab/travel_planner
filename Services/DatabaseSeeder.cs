using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TravelPlanner.Data;
using TravelPlanner.Models;

namespace TravelPlanner.Services
{
    /// <summary>
    /// Database Seeder for initial data
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task SeedDefaultUsersAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = services.GetRequiredService<IConfiguration>();
            var adminPassword = configuration["AdminAccount:Password"]
                ?? throw new InvalidOperationException("AdminAccount:Password is not configured.");

            string[] roles = ["Admin", "User"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var demoUsers = new[]
            {
                new { Email = "test@example.com", Password = "Test@123", FirstName = "Test", LastName = "User", Role = "User" },
                new { Email = "admin@travelplannerbd.com", Password = adminPassword, FirstName = "Admin", LastName = "User", Role = "Admin" }
            };

            foreach (var demoUser in demoUsers)
            {
                var user = await userManager.FindByEmailAsync(demoUser.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = demoUser.Email,
                        Email = demoUser.Email,
                        FirstName = demoUser.FirstName,
                        LastName = demoUser.LastName,
                        EmailConfirmed = true,
                        IsVerified = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(user, demoUser.Password);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to create demo user {demoUser.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                if (demoUser.Role == "Admin")
                {
                    await userManager.RemovePasswordAsync(user);
                    var passwordResult = await userManager.AddPasswordAsync(user, demoUser.Password);
                    if (!passwordResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to set admin password: {string.Join(", ", passwordResult.Errors.Select(e => e.Description))}");
                    }
                }
                else if (!user.IsVerified)
                {
                    user.IsActive = true;
                    user.IsVerified = true;
                    await userManager.UpdateAsync(user);
                }

                if (!await userManager.IsInRoleAsync(user, demoUser.Role))
                {
                    await userManager.AddToRoleAsync(user, demoUser.Role);
                }
            }
        }

        public static void SeedDatabase(ApplicationDbContext context)
        {
            // Check if data already exists
            if (context.Destinations.Any())
                return;

            // Seed Destinations
            var destinations = new List<Destination>
            {
                new Destination
                {
                    Name = "Paris",
                    Country = "France",
                    City = "Paris",
                    Description = "Experience the magic of Paris, the City of Light. From the iconic Eiffel Tower to charming cafes and world-class museums, Paris offers unforgettable romantic experiences.",
                    ImageUrl = "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?q=80&w=900&auto=format&fit=crop",
                    Latitude = 48.8566m,
                    Longitude = 2.3522m,
                    WeatherType = "Moderate",
                    BestTimeToVisit = "5",
                    Category = "City",
                    AverageRating = 4.8m,
                    ReviewCount = 245,
                    IsPopular = true
                },
                new Destination
                {
                    Name = "Bali",
                    Country = "Indonesia",
                    City = "Denpasar",
                    Description = "Discover the beauty of Bali with its pristine beaches, lush rice terraces, and rich cultural heritage. Perfect for relaxation and adventure.",
                    ImageUrl = "https://images.unsplash.com/photo-1537996194471-e657df975ab4?q=80&w=900&auto=format&fit=crop",
                    Latitude = -8.6705m,
                    Longitude = 115.2126m,
                    WeatherType = "Tropical",
                    BestTimeToVisit = "4",
                    Category = "Beach",
                    AverageRating = 4.7m,
                    ReviewCount = 312,
                    IsPopular = true
                },
                new Destination
                {
                    Name = "Tokyo",
                    Country = "Japan",
                    City = "Tokyo",
                    Description = "Visit the vibrant capital of Japan where ancient traditions meet modern technology. Experience incredible food, culture, and entertainment.",
                    ImageUrl = "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?q=80&w=900&auto=format&fit=crop",
                    Latitude = 35.6762m,
                    Longitude = 139.6503m,
                    WeatherType = "Temperate",
                    BestTimeToVisit = "3",
                    Category = "City",
                    AverageRating = 4.9m,
                    ReviewCount = 456,
                    IsPopular = true
                },
                new Destination
                {
                    Name = "Swiss Alps",
                    Country = "Switzerland",
                    City = "Interlaken",
                    Description = "Explore the majestic Swiss Alps with breathtaking mountain views, adventure sports, and charming alpine villages.",
                    ImageUrl = "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?q=80&w=900&auto=format&fit=crop",
                    Latitude = 46.6556m,
                    Longitude = 8.1791m,
                    WeatherType = "Cool",
                    BestTimeToVisit = "7",
                    Category = "Mountain",
                    AverageRating = 4.9m,
                    ReviewCount = 189,
                    IsPopular = true
                },
                new Destination
                {
                    Name = "New York",
                    Country = "United States",
                    City = "New York",
                    Description = "The city that never sleeps! Experience world-class attractions, Broadway shows, iconic landmarks, and diverse cuisine.",
                    ImageUrl = "https://images.unsplash.com/photo-1496588152823-86ff7695e68f?q=80&w=900&auto=format&fit=crop",
                    Latitude = 40.7128m,
                    Longitude = -74.0060m,
                    WeatherType = "Temperate",
                    BestTimeToVisit = "9",
                    Category = "City",
                    AverageRating = 4.6m,
                    ReviewCount = 523,
                    IsPopular = true
                }
            };

            context.Destinations.AddRange(destinations);
            context.SaveChanges();

            // Seed Attractions
            var attractions = new List<Attraction>
            {
                new Attraction
                {
                    DestinationId = 1,
                    Name = "Eiffel Tower",
                    Description = "The iconic iron lattice tower is the most visited monument in Paris.",
                    ImageUrl = "https://via.placeholder.com/300x200?text=Eiffel+Tower",
                    Latitude = 48.8584m,
                    Longitude = 2.2945m,
                    Category = "Monument",
                    EntryFee = "$15",
                    OpeningHours = "9:00 AM - 12:45 AM",
                    Rating = 4.9m
                },
                new Attraction
                {
                    DestinationId = 1,
                    Name = "Louvre Museum",
                    Description = "Home to the Mona Lisa and thousands of other masterpieces.",
                    ImageUrl = "https://via.placeholder.com/300x200?text=Louvre",
                    Latitude = 48.8606m,
                    Longitude = 2.3352m,
                    Category = "Museum",
                    EntryFee = "$18",
                    OpeningHours = "9:00 AM - 6:00 PM",
                    Rating = 4.7m
                }
            };

            context.Attractions.AddRange(attractions);
            context.SaveChanges();
        }
    }
}
