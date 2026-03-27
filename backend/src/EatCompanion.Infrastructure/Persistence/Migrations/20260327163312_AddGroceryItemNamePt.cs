using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatCompanion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGroceryItemNamePt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NamePt",
                table: "grocery_items",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NamePt",
                table: "grocery_items");
        }
    }
}
