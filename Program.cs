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
builder.Configuration.AddUserSecrets<Program>(optional: true);

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

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
        builder.AllowAnyOrigin()
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
builder.Services.AddHttpClient("TravelAi", client => client.Timeout = TimeSpan.FromSeconds(45));
builder.Services.AddScoped<IChatService, ChatService>();

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

// Create or update database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.CanConnect() && dbContext.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM sys.tables WHERE name = 'Destinations'").Single() == 0)
    {
        dbContext.Database.EnsureDeleted();
    }
    dbContext.Database.EnsureCreated();
    dbContext.Database.ExecuteSqlRaw("IF COL_LENGTH('Destinations', 'IsPopular') IS NULL ALTER TABLE Destinations ADD IsPopular bit NOT NULL CONSTRAINT DF_Destinations_IsPopular DEFAULT 0 WITH VALUES");
    dbContext.Database.ExecuteSqlRaw("IF COL_LENGTH('Destinations', 'IsPublished') IS NULL ALTER TABLE Destinations ADD IsPublished bit NOT NULL CONSTRAINT DF_Destinations_IsPublished DEFAULT 1 WITH VALUES");
    dbContext.Database.ExecuteSqlRaw("UPDATE Destinations SET IsPopular = 1 WHERE Name IN ('Paris', 'Bali', 'Tokyo', 'Swiss Alps', 'New York') AND IsPopular = 0");
    dbContext.Database.ExecuteSqlRaw("IF COL_LENGTH('Reviews', 'TripId') IS NULL ALTER TABLE Reviews ADD TripId int NULL");
    dbContext.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Reviews_Trips_TripId') ALTER TABLE Reviews ADD CONSTRAINT FK_Reviews_Trips_TripId FOREIGN KEY (TripId) REFERENCES Trips(Id) ON DELETE CASCADE");
    dbContext.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reviews_TripId_UserId' AND object_id = OBJECT_ID('Reviews')) CREATE INDEX IX_Reviews_TripId_UserId ON Reviews(TripId, UserId)");
    dbContext.Database.ExecuteSqlRaw("IF OBJECT_ID('Payments', 'U') IS NULL CREATE TABLE Payments (Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY, UserId nvarchar(450) NOT NULL, TripId int NOT NULL, TransactionId nvarchar(100) NOT NULL, Amount decimal(18,2) NOT NULL, Currency nvarchar(10) NOT NULL, PaymentMethod nvarchar(50) NOT NULL, Status nvarchar(30) NOT NULL, GatewayResponse nvarchar(max) NULL, CreatedAt datetime2 NOT NULL, PaidAt datetime2 NULL, CONSTRAINT FK_Payments_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION, CONSTRAINT FK_Payments_Trips_TripId FOREIGN KEY (TripId) REFERENCES Trips(Id) ON DELETE CASCADE)");
    dbContext.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Payments_TripId' AND object_id = OBJECT_ID('Payments')) CREATE INDEX IX_Payments_TripId ON Payments(TripId)");
    DatabaseSeeder.SeedDatabase(dbContext);
    await DatabaseSeeder.SeedDefaultUsersAsync(scope.ServiceProvider);
}

Console.WriteLine("🚀 Travel Planner running on http://localhost:8000");
app.Run("http://localhost:8000");
