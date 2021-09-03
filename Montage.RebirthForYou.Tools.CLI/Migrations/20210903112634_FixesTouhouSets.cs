using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class FixesTouhouSets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 22,
                column: "IsDone",
                value: true);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 27,
                column: "IsDone",
                value: true);

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 57, 0, new DateTime(2021, 9, 3, 20, 0, 0, 0, DateTimeKind.Unspecified), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TH_001B.r4uset" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 58, 0, new DateTime(2021, 9, 3, 20, 0, 0, 0, DateTimeKind.Unspecified), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TH_001T.r4uset" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 58);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 22,
                column: "IsDone",
                value: false);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 27,
                column: "IsDone",
                value: false);
        }
    }
}
