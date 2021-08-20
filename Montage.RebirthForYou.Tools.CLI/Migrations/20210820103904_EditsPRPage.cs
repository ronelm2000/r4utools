using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class EditsPRPage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 18,
                columns: new[] { "DateAdded", "IsDone" },
                values: new object[] { new DateTime(2021, 8, 20, 18, 34, 0, 0, DateTimeKind.Unspecified), true });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 31, 0, new DateTime(2021, 8, 20, 18, 34, 0, 0, DateTimeKind.Unspecified), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/PR.r4uset" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 31);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 18,
                columns: new[] { "DateAdded", "IsDone" },
                values: new object[] { new DateTime(2020, 1, 2, 0, 0, 10, 0, DateTimeKind.Unspecified), false });
        }
    }
}
