using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TravelPlanner.Models;

namespace TravelPlanner.Data
{
    /// <summary>
    /// Application Database Context
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Trip> Trips { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Itinerary> Itineraries { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Accommodation> Accommodations { get; set; }
        public DbSet<Transportation> Transportations { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<PackingItem> PackingItems { get; set; }
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<DestinationImage> DestinationImages { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Trip>().Property(t => t.BudgetLimit).HasPrecision(18, 2);
            modelBuilder.Entity<Payment>().Property(payment => payment.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Accommodation>().Property(a => a.PricePerNight).HasPrecision(18, 2);
            modelBuilder.Entity<Accommodation>().Property(a => a.TotalCost).HasPrecision(18, 2);
            modelBuilder.Entity<Accommodation>().Property(a => a.HotelRating).HasPrecision(3, 2);
            modelBuilder.Entity<Transportation>().Property(t => t.Cost).HasPrecision(18, 2);
            modelBuilder.Entity<Expense>().Property(e => e.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Destination>().Property(d => d.Latitude).HasPrecision(9, 6);
            modelBuilder.Entity<Destination>().Property(d => d.Longitude).HasPrecision(9, 6);
            modelBuilder.Entity<Destination>().Property(d => d.AverageRating).HasPrecision(3, 2);
            modelBuilder.Entity<Attraction>().Property(a => a.Latitude).HasPrecision(9, 6);
            modelBuilder.Entity<Attraction>().Property(a => a.Longitude).HasPrecision(9, 6);
            modelBuilder.Entity<Attraction>().Property(a => a.Rating).HasPrecision(3, 2);

            modelBuilder.Entity<Trip>().Ignore(t => t.TotalSpent);
            modelBuilder.Entity<Trip>().Ignore(t => t.CoverImageUrl);
            modelBuilder.Entity<Trip>().Ignore(t => t.IsPublic);
            modelBuilder.Entity<Trip>().Ignore(t => t.UpdatedAt);

            modelBuilder.Entity<Destination>().Ignore(d => d.State);
            modelBuilder.Entity<Destination>().Ignore(d => d.City);
            modelBuilder.Entity<Destination>().Ignore(d => d.Latitude);
            modelBuilder.Entity<Destination>().Ignore(d => d.Longitude);
            modelBuilder.Entity<Destination>().Ignore(d => d.WeatherType);
            modelBuilder.Entity<Destination>().Ignore(d => d.ReviewCount);

            // Configure Trip relationships
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.User)
                .WithMany(u => u.Trips)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Destination)
                .WithMany(d => d.Trips)
                .HasForeignKey(t => t.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Itinerary relationships
            modelBuilder.Entity<Itinerary>()
                .HasOne(i => i.Trip)
                .WithMany(t => t.Itineraries)
                .HasForeignKey(i => i.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Activity relationships
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.Itinerary)
                .WithMany(i => i.Activities)
                .HasForeignKey(a => a.ItineraryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Accommodation relationships
            modelBuilder.Entity<Accommodation>()
                .HasOne(a => a.Trip)
                .WithMany(t => t.Accommodations)
                .HasForeignKey(a => a.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Transportation relationships
            modelBuilder.Entity<Transportation>()
                .HasOne(t => t.Trip)
                .WithMany(tr => tr.Transportations)
                .HasForeignKey(t => t.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Expense relationships and legacy mapped columns.
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Trip)
                .WithMany(t => t.Expenses)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Expense>()
                .Property(e => e.Description)
                .HasColumnName("Name");

            modelBuilder.Entity<Expense>()
                .Property(e => e.ExpenseDate)
                .HasColumnName("IncurredAt");

            modelBuilder.Entity<Expense>()
                .Ignore(e => e.Category);

            modelBuilder.Entity<Expense>()
                .Ignore(e => e.PaymentMethod);

            modelBuilder.Entity<Expense>()
                .Ignore(e => e.IsPaid);

            modelBuilder.Entity<Expense>()
                .Ignore(e => e.Notes);

            modelBuilder.Entity<Expense>()
                .Ignore(e => e.CreatedAt);

            // Configure PackingItem relationships
            modelBuilder.Entity<PackingItem>()
                .HasOne(p => p.Trip)
                .WithMany(t => t.PackingItems)
                .HasForeignKey(p => p.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Attraction relationships
            modelBuilder.Entity<Attraction>()
                .HasOne(a => a.Destination)
                .WithMany(d => d.Attractions)
                .HasForeignKey(a => a.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Review relationships
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Destination)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Trip)
                .WithMany(t => t.Reviews)
                .HasForeignKey(r => r.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Favorite relationships
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Destination)
                .WithMany(d => d.Favorites)
                .HasForeignKey(f => f.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure DestinationImage relationships
            modelBuilder.Entity<DestinationImage>()
                .HasOne(di => di.Destination)
                .WithMany(d => d.Images)
                .HasForeignKey(di => di.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(payment => payment.User)
                .WithMany()
                .HasForeignKey(payment => payment.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(payment => payment.Trip)
                .WithMany()
                .HasForeignKey(payment => payment.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
