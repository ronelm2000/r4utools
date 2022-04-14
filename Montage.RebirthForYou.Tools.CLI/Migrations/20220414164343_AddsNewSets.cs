using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class AddsNewSets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 78, 0, new DateTime(2022, 4, 14, 14, 26, 0, 0, DateTimeKind.Unspecified), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_SSSS_002B.r4uset" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 79, 0, new DateTime(2022, 4, 14, 14, 26, 0, 0, DateTimeKind.Unspecified), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_AL_002B.r4uset" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 80, 0, new DateTime(2022, 4, 14, 14, 26, 0, 0, DateTimeKind.Unspecified), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_002E.r4uset" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 81, 0, new DateTime(2022, 4, 14, 14, 26, 0, 0, DateTimeKind.Unspecified), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_BKRM_001B.r4uset" });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 82, 0, new DateTime(2022, 4, 14, 14, 26, 0, 0, DateTimeKind.Unspecified), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_BKRM_001T.r4uset" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 82);
        }
    }
}
