using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mistria.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCityToDayTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "DayTrips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "DayTrips");
        }
    }
}
