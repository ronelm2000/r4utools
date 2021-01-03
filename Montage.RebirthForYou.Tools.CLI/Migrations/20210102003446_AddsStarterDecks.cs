using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class AddsStarterDecks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 15, 0, new DateTime(2021, 1, 2, 0, 0, 7, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22Rebirth%22" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 16, 0, new DateTime(2021, 1, 2, 0, 0, 8, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22BanG_Dream!_Girls_Band_Party!%E2%98%86PICO%22" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 17, 0, new DateTime(2021, 1, 2, 0, 0, 9, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22Revue_Starlight_-Re_LIVE-%22" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 17);
        }
    }
}
