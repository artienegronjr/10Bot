using Microsoft.EntityFrameworkCore.Migrations;

namespace _10Bot.Migrations
{
    public partial class Scorereporting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PreviousSkillRating",
                table: "Users",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RatingsDeviation",
                table: "Users",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Volatility",
                table: "Users",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousSkillRating",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RatingsDeviation",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Volatility",
                table: "Users");
        }
    }
}
