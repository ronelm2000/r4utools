using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class AddsBatch1TD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 18, 0, new DateTime(2021, 01, 02, 0, 0, 10, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Rebirth" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 19, 0, new DateTime(2021, 01, 02, 0, 0, 11, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_BanG_Dream!_Girls_Band_Party!%E2%98%86PICO" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 20, 0, new DateTime(2021, 01, 02, 0, 0, 12, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Isekai_Quartet" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 21, 0, new DateTime(2021, 01, 02, 0, 0, 13, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Touhou_Project" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 22, 0, new DateTime(2021, 01, 02, 0, 0, 14, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Revue_Starlight_-Re_LIVE-" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 22);
        }
    }
}
