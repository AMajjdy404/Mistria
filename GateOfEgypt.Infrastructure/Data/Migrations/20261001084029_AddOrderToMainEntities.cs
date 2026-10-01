using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateOfEgypt.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderToMainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Weddings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Founders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Blogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Activities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill existing rows with sequential, non-colliding order values (by Id) instead of
            // leaving them all at the default 0, which would violate the app's order-uniqueness rule.
            foreach (var table in new[] { "Weddings", "Events", "Activities", "Services", "Founders", "Blogs" })
            {
                migrationBuilder.Sql($@"
                UPDATE t
                SET t.[Order] = sub.rn
                FROM [{table}] t
                INNER JOIN (SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS rn FROM [{table}]) sub ON t.Id = sub.Id;
            ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Weddings");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Founders");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Activities");
        }
    }
}
