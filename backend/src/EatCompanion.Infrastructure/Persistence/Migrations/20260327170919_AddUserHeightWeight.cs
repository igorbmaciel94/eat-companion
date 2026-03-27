using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatCompanion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserHeightWeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HeightCm",
                table: "users",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "users",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "users");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "users");
        }
    }
}
