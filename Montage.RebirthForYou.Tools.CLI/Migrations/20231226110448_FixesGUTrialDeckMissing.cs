using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    /// <inheritdoc />
    public partial class FixesGUTrialDeckMissing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_R4UCards_Traits_R4UCards_R4UCardSerial",
                table: "R4UCards_Traits");

            migrationBuilder.AlterColumn<string>(
                name: "R4UCardSerial",
                table: "R4UCards_Traits",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 124,
                column: "IsDone",
                value: false);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 126,
                column: "IsDone",
                value: false);

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 127, 0, new DateTime(2023, 12, 26, 14, 48, 49, 611, DateTimeKind.Unspecified).AddTicks(3309), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GU_001T.r4uset" });

            migrationBuilder.AddForeignKey(
                name: "FK_R4UCards_Traits_R4UCards_R4UCardSerial",
                table: "R4UCards_Traits",
                column: "R4UCardSerial",
                principalTable: "R4UCards",
                principalColumn: "Serial");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_R4UCards_Traits_R4UCards_R4UCardSerial",
                table: "R4UCards_Traits");

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 127);

            migrationBuilder.AlterColumn<string>(
                name: "R4UCardSerial",
                table: "R4UCards_Traits",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 124,
                column: "IsDone",
                value: true);

            migrationBuilder.UpdateData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 126,
                column: "IsDone",
                value: true);

            migrationBuilder.AddForeignKey(
                name: "FK_R4UCards_Traits_R4UCards_R4UCardSerial",
                table: "R4UCards_Traits",
                column: "R4UCardSerial",
                principalTable: "R4UCards",
                principalColumn: "Serial",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
