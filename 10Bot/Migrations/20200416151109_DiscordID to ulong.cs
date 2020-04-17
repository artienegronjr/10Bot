using Microsoft.EntityFrameworkCore.Migrations;

namespace _10Bot.Migrations
{
    public partial class DiscordIDtoulong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "DiscordID",
                table: "Users",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "DiscordID",
                table: "Users",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal));
        }
    }
}
