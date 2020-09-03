using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class RedoGochiUsa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 1,
                columns: new[] { "DateAdded", "IsDone" },
                values: new object[] { new DateTime(2020, 9, 2, 0, 0, 23, 534, DateTimeKind.Local).AddTicks(9446), false });

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 2,
                columns: new[] { "DateAdded", "IsDone" },
                values: new object[] { new DateTime(2020, 9, 2, 0, 0, 23, 534, DateTimeKind.Local).AddTicks(9446), false });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 1,
                columns: new[] { "DateAdded", "IsDone" },
                values: new object[] { new DateTime(2020, 8, 13, 17, 7, 23, 534, DateTimeKind.Local).AddTicks(9446), false });

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 2,
                columns: new[] { "DateAdded", "IsDone" },
                values: new object[] { new DateTime(2020, 8, 13, 17, 7, 23, 536, DateTimeKind.Local).AddTicks(1375), false });
        }
    }
}
