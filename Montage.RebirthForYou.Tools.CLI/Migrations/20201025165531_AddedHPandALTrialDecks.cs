using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class AddedHPandALTrialDecks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 9, 0, new DateTime(2020, 10, 26, 0, 0, 1, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._3rd_Gen)" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 10, 0, new DateTime(2020, 10, 26, 0, 0, 2, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._4th_Gen)" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 11, 0, new DateTime(2020, 10, 26, 0, 0, 3, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Azur_Lane" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 11);
        }
    }
}
