using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class AddsPRSets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 14, 0, new DateTime(2021, 01, 02, 0, 0, 6, 0, DateTimeKind.Local).AddTicks(9446), false, "https://raw.githubusercontent.com/unsiga25/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/PR.r4uset" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 14);
        }
    }
}
