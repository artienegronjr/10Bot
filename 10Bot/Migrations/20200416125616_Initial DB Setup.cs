using Microsoft.EntityFrameworkCore.Migrations;

namespace _10Bot.Migrations
{
    public partial class InitialDBSetup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordID = table.Column<long>(nullable: true),
                    Username = table.Column<string>(nullable: true),
                    SkillRating = table.Column<double>(nullable: false),
                    Wins = table.Column<int>(nullable: false),
                    Losses = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
