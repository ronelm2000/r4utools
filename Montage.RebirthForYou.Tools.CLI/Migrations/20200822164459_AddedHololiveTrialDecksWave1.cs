using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class AddedHololiveTrialDecksWave1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 3, 0, new DateTime(2020, 8, 23, 0, 43, 53, 205, DateTimeKind.Local), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._0th_Gen)" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 4, 0, new DateTime(2020, 8, 23, 0, 43, 53, 206, DateTimeKind.Local), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._1st_Gen)" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 4);
        }
    }
}
