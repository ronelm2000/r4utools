using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class AddsMultipleBPsTDsAndPRsByUnsiga27 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 13, 0, new DateTime(2020, 12, 21, 0, 0, 4, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Azur_Lane" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 14, 0, new DateTime(2020, 12, 21, 0, 0, 5, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Hololive_Production" });

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

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 18, 0, new DateTime(2020, 1, 2, 0, 0, 10, 0, DateTimeKind.Unspecified), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/PR.r4uset" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 19, 0, new DateTime(2020, 1, 2, 0, 0, 11, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Rebirth" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 20, 0, new DateTime(2021, 1, 2, 0, 0, 12, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_BanG_Dream!_Girls_Band_Party!%E2%98%86PICO" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 21, 0, new DateTime(2021, 1, 2, 1, 0, 12, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Isekai_Quartet" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 22, 0, new DateTime(2021, 1, 2, 1, 0, 13, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Touhou_Project" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 23, 0, new DateTime(2021, 1, 2, 1, 0, 14, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Revue_Starlight_-Re_LIVE-" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 24, 0, new DateTime(2021, 1, 2, 1, 0, 15, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Rebirth" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 25, 0, new DateTime(2021, 1, 2, 1, 0, 16, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_BanG_Dream!_Girls_Band_Party!%E2%98%86PICO" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 26, 0, new DateTime(2021, 1, 2, 1, 0, 17, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Isekai_Quartet" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 27, 0, new DateTime(2021, 1, 2, 1, 0, 18, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Touhou_Project" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 28, 0, new DateTime(2021, 1, 2, 1, 0, 19, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Revue_Starlight_-Re_LIVE-" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 14);

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

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 28);
        }
    }
}
