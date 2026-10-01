using System.Reflection.Emit;
using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GateOfEgypt.Domain.Models;

namespace GateOfEgypt.Infrastructure.Data
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
        public DbSet<AuditLog> AuditLogs { get; set; }

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

                // Pricing tiers (with/without hotels options, each with date ranges / group pricing / accommodation) stored as a single JSON column
                entity.OwnsMany(p => p.PricingTiers, tier =>
                {
                    tier.ToJson();
                    tier.OwnsOne(t => t.WithHotels, ConfigurePricingOption);
                    tier.OwnsOne(t => t.WithoutHotels, ConfigurePricingOption);
                });
            });

            builder.Entity<Destination>(entity =>
            {
                // Itinerary days (and their nested events) stored as a single JSON column
                entity.OwnsMany(d => d.Itinerary, day =>
                {
                    day.ToJson();
                    day.OwnsMany(d => d.Events);
                });

                // Pricing tiers (and their nested date ranges / group pricing) stored as a single JSON column
                entity.OwnsMany(d => d.PricingTiers, tier =>
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

        // Shared mapping for a tier's "with hotels" / "without hotels" option (date ranges + accommodation)
        private static void ConfigurePricingOption(OwnedNavigationBuilder<ProgramPricingTier, PricingOption> option)
        {
            option.OwnsMany(o => o.DateRanges, range =>
            {
                range.OwnsMany(r => r.GroupPricing, gp =>
                {
                    gp.Property(g => g.PricePerPerson).HasPrecision(18, 2);
                });
            });
            option.OwnsMany(o => o.AccommodationOptions);
        }
    }
}
