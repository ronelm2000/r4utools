using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class AddedHPandALSets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 12, 0, new DateTime(2020, 12, 21, 0, 0, 4, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Azur_Lane" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 13, 0, new DateTime(2020, 12, 21, 0, 0, 5, 0, DateTimeKind.Unspecified), false, "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Hololive_Production" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 13);
        }
    }
}
