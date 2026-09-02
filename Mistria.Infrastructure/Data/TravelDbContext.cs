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
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Wedding> Weddings { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<AboutUs> AboutUsInfos { get; set; }
        public DbSet<Founder> Founders { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogSub> BlogSubs { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewPlatform> ReviewPlatforms { get; set; }
        public DbSet<Reel> Reels { get; set; }
        public DbSet<CustomerPhoto> CustomerPhotos { get; set; }
        public DbSet<ReviewsSettings> ReviewsSettingsInfos { get; set; }
        public DbSet<SocialMediaLink> SocialMediaLinks { get; set; }

        public TravelDbContext(DbContextOptions<TravelDbContext> options):base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TravelProgram>(entity =>
            {
                // Itinerary days (and their nested events) stored as a single JSON column
                entity.OwnsMany(p => p.Itinerary, day =>
                {
                    day.ToJson();
                    day.OwnsMany(d => d.Events);
                });

                // Pricing tiers (and their nested date ranges / group pricing) stored as a single JSON column
                entity.OwnsMany(p => p.PricingTiers, tier =>
                {
                    tier.ToJson();
                    tier.OwnsMany(t => t.DateRanges, range =>
                    {
                        range.OwnsMany(r => r.GroupPricing, gp =>
                        {
                            gp.Property(g => g.PricePerPerson).HasPrecision(18, 2);
                        });
                    });
                });
            });

            builder.Entity<Destination>()
                   .Property(p => p.PricePerPerson)
                   .HasPrecision(18, 2);

            builder.Entity<Destination>(entity =>
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

            builder.Entity<BlogSub>(entity =>
            {
                // Configure Content as JSON
                entity.Property(e => e.Content)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = true }),
                        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions()) ?? new Dictionary<string, string>(),
                        new ValueComparer<Dictionary<string, string>>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, p) => HashCode.Combine(a, p.Key.GetHashCode(), p.Value.GetHashCode())),
                            c => c.ToDictionary(p => p.Key, p => p.Value)))
                    .HasColumnType("nvarchar(max)"); // Store as JSON string
            });


            base.OnModelCreating(builder);
        }
    }
}
