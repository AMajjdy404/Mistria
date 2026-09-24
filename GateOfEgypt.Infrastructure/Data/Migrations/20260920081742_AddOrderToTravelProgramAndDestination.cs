using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateOfEgypt.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderToTravelProgramAndDestination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "TravelPrograms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Destinations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill existing rows with sequential, non-colliding order values (by Id) instead of
            // leaving them all at the default 0, which would violate the app's order-uniqueness rule.
            migrationBuilder.Sql(@"
                UPDATE t
                SET t.[Order] = sub.rn
                FROM TravelPrograms t
                INNER JOIN (SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS rn FROM TravelPrograms) sub ON t.Id = sub.Id;
            ");

            migrationBuilder.Sql(@"
                UPDATE d
                SET d.[Order] = sub.rn
                FROM Destinations d
                INNER JOIN (SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS rn FROM Destinations) sub ON d.Id = sub.Id;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "TravelPrograms");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Destinations");
        }
    }
}
