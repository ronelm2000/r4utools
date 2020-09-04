using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class RedoHololive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 1,
                column: "IsDone",
                value: false);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 2,
                column: "IsDone",
                value: false);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 3,
                column: "IsDone",
                value: true);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 4,
                column: "IsDone",
                value: true);

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 5, 0, new DateTime(2020, 9, 4, 0, 43, 53, 205, DateTimeKind.Local), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._0th_Gen)" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 6, 0, new DateTime(2020, 9, 4, 0, 43, 53, 206, DateTimeKind.Local), false, "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._1st_Gen)" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 1,
                column: "IsDone",
                value: true);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 2,
                column: "IsDone",
                value: true);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 3,
                column: "IsDone",
                value: false);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 4,
                column: "IsDone",
                value: false);
        }
    }
}
