using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateOfEgypt.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitProgramPricingTiersByHotelsAndAddAccommodation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PricingTiers is a JSON column, so there is no schema change. Existing program tiers kept their
            // DateRanges directly on the tier; move them under WithHotels and start WithoutHotels empty.
            migrationBuilder.Sql(@"
UPDATE [TravelPrograms]
SET [PricingTiers] = ISNULL((
    SELECT
        t.[Name] AS [Name],
        t.[IsMostChosen] AS [IsMostChosen],
        JSON_QUERY(CONCAT(N'{""DateRanges"":', ISNULL(t.[DateRanges], N'[]'), N',""AccommodationOptions"":[]}')) AS [WithHotels],
        JSON_QUERY(N'{""DateRanges"":[],""AccommodationOptions"":[]}') AS [WithoutHotels]
    FROM OPENJSON([PricingTiers]) WITH (
        [Name] nvarchar(max) '$.Name',
        [IsMostChosen] bit '$.IsMostChosen',
        [DateRanges] nvarchar(max) '$.DateRanges' AS JSON
    ) AS t
    FOR JSON PATH
), N'[]')
WHERE [PricingTiers] IS NOT NULL AND ISJSON([PricingTiers]) = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the old shape: DateRanges back on the tier (taken from WithHotels).
            // WithoutHotels prices and accommodation options are dropped.
            migrationBuilder.Sql(@"
UPDATE [TravelPrograms]
SET [PricingTiers] = ISNULL((
    SELECT
        t.[Name] AS [Name],
        t.[IsMostChosen] AS [IsMostChosen],
        JSON_QUERY(ISNULL(t.[DateRanges], N'[]')) AS [DateRanges]
    FROM OPENJSON([PricingTiers]) WITH (
        [Name] nvarchar(max) '$.Name',
        [IsMostChosen] bit '$.IsMostChosen',
        [DateRanges] nvarchar(max) '$.WithHotels.DateRanges' AS JSON
    ) AS t
    FOR JSON PATH
), N'[]')
WHERE [PricingTiers] IS NOT NULL AND ISJSON([PricingTiers]) = 1;");
        }
    }
}
