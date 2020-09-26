using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class AddHololiveTrialDecksWave2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 7, 0, new DateTime(2020, 9, 25, 0, 43, 53, 206, DateTimeKind.Local), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_%28ver._2nd_Gen%29" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 8, 0, new DateTime(2020, 9, 25, 0, 43, 53, 206, DateTimeKind.Local), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._GAMERS)" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 8);
        }
    }
}
