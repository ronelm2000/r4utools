using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class AddsHololiveSpecialSet01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 45,
                column: "IsDone",
                value: true);

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 47, 0, new DateTime(2021, 8, 28, 15, 16, 0, 0, DateTimeKind.Unspecified), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_001E.r4uset" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 46, 0, new DateTime(2021, 8, 28, 15, 16, 0, 0, DateTimeKind.Unspecified), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_001B.r4uset" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 47);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 45,
                column: "IsDone",
                value: false);
        }
    }
}
