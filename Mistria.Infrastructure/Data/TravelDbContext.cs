using System.Reflection.Emit;
using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Mistria.Domain.Models;

namespace Mistria.Infrastructure.Data
{
    public class TravelDbContext: IdentityDbContext<AppUser>
    {
        public DbSet<TravelProgram> TravelPrograms { get; set; }
        public DbSet<DayTrip> DayTrips { get; set; }
        public DbSet<Wedding> Weddings { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Service> Services { get; set; }

        public TravelDbContext(DbContextOptions<TravelDbContext> options):base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TravelProgram>()
                   .Property(p => p.PricePerPerson)
                   .HasPrecision(18, 2);

            builder.Entity<TravelProgram>(entity =>
            {
                // Configure Itinerary as JSON
                entity.Property(e => e.Itinerary)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = true }),
                        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions()) ?? new Dictionary<string, string>(),
                        new ValueComparer<Dictionary<string, string>>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, p) => HashCode.Combine(a, p.Key.GetHashCode(), p.Value.GetHashCode())),
                            c => c.ToDictionary(p => p.Key, p => p.Value)))
                    .HasColumnType("nvarchar(max)"); // Store as JSON string
            });

            builder.Entity<DayTrip>()
                   .Property(p => p.PricePerPerson)
                   .HasPrecision(18, 2);

            builder.Entity<DayTrip>(entity =>
            {
                // Configure Itinerary as JSON
                entity.Property(e => e.Itinerary)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = true }),
                        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions()) ?? new Dictionary<string, string>(),
                        new ValueComparer<Dictionary<string, string>>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, p) => HashCode.Combine(a, p.Key.GetHashCode(), p.Value.GetHashCode())),
                            c => c.ToDictionary(p => p.Key, p => p.Value)))
                    .HasColumnType("nvarchar(max)"); // Store as JSON string

            });

            builder.Entity<Activity>()
               .Property(p => p.Price)
               .HasPrecision(18, 2);

            builder.Entity<Service>()
                   .Property(p => p.Price)
                   .HasPrecision(18, 2);


            base.OnModelCreating(builder);
        }
    }
}
