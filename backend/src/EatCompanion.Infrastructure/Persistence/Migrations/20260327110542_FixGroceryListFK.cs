using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EatCompanion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixGroceryListFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_grocery_lists_meal_plans_MealPlanId1",
                table: "grocery_lists");

            migrationBuilder.DropIndex(
                name: "IX_grocery_lists_MealPlanId1",
                table: "grocery_lists");

            migrationBuilder.DropColumn(
                name: "MealPlanId1",
                table: "grocery_lists");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MealPlanId1",
                table: "grocery_lists",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_grocery_lists_MealPlanId1",
                table: "grocery_lists",
                column: "MealPlanId1");

            migrationBuilder.AddForeignKey(
                name: "FK_grocery_lists_meal_plans_MealPlanId1",
                table: "grocery_lists",
                column: "MealPlanId1",
                principalTable: "meal_plans",
                principalColumn: "Id");
        }
    }
}
