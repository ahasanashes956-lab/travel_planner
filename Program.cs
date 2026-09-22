using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TravelPlanner.Data;
using TravelPlanner.Models;
using TravelPlanner.Services;
using TravelPlanner.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Identity services
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedEmail = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure authentication to return 401 for API requests instead of redirecting
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        // Check if this is an API request
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    };
});

// Add MVC and controllers
builder.Services.AddControllersWithViews();

// Add CORS for frontend communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
         builder.WithOrigins("http://localhost:8000", "http://127.0.0.1:8000")
             .AllowCredentials()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Add repositories
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IDestinationRepository, DestinationRepository>();
builder.Services.AddScoped<IAccommodationRepository, AccommodationRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;

        await context.Response.WriteAsJsonAsync(new { 
            message = "An internal server error occurred", 
            details = ex.Message 
        });
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Enable CORS
app.UseCors("AllowFrontend");

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Protect the static admin console with the real Identity session and role.
app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/admin.html", StringComparison.OrdinalIgnoreCase) &&
        (!context.User.Identity?.IsAuthenticated ?? true || !context.User.IsInRole("Admin")))
    {
        context.Response.Redirect("/login.html?returnUrl=%2Fadmin.html");
        return;
    }

    await next();
});

// Serve static files after authentication so admin.html cannot bypass the guard.
app.UseStaticFiles();

var frontendPath = Path.Combine(Directory.GetCurrentDirectory(), "Frontend");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(frontendPath),
    RequestPath = ""
});

// Map controller routes
app.MapControllers(); // Enable attribute-based routing (for /api/* endpoints)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Default route - serve index.html for root
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(Path.Combine(frontendPath, "index.html"));
});
    
app.MapGet("/favicon.ico", async context =>
{
    context.Response.ContentType = "image/svg+xml";
    await context.Response.SendFileAsync(Path.Combine(frontendPath, "images", "travel-placeholder.svg"));
});

// Fallback handler for SPA - serve requested file or index.html (exclude API routes)
app.MapFallback(async context =>
{
    var path = context.Request.Path.Value;
    
    // Skip API routes
    if (path != null && path.StartsWith("/api/"))
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Not Found");
        return;
    }
    
    var filePath = Path.Combine(frontendPath, path.TrimStart('/'));
    
    if (File.Exists(filePath) && !Directory.Exists(filePath))
    {
        await context.Response.SendFileAsync(filePath);
    }
    else
    {
        // Serve index.html for SPA routing
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(frontendPath, "index.html"));
    }
});

// Create and seed the initial PostgreSQL database. Add EF migrations before
// evolving an already-populated production schema.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    DatabaseSeeder.SeedDatabase(dbContext);
    await DatabaseSeeder.SeedDefaultUsersAsync(scope.ServiceProvider);
}

Console.WriteLine("🚀 Travel Planner running on http://localhost:8000");
var port = Environment.GetEnvironmentVariable("PORT") ?? "8000";
Console.WriteLine($"Travel Planner running on port {port}");
app.Run($"http://0.0.0.0:{port}");
