using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateOfEgypt.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameLocationUrlToDurationInTravelProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LocationUrl",
                table: "TravelPrograms",
                newName: "Duration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "TravelPrograms",
                newName: "LocationUrl");
        }
    }
}
