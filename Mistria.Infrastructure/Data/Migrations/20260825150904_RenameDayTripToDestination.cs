using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mistria.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameDayTripToDestination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "DayTrips",
                newName: "Destinations");

            migrationBuilder.Sql("EXEC sp_rename N'PK_DayTrips', N'PK_Destinations';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("EXEC sp_rename N'PK_Destinations', N'PK_DayTrips';");

            migrationBuilder.RenameTable(
                name: "Destinations",
                newName: "DayTrips");
        }
    }
}
