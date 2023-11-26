using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    /// <inheritdoc />
    public partial class AddsHSandGPC : Migration
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

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[,]
                {
                    { 97, 0, new DateTime(2023, 11, 26, 23, 26, 2, 902, DateTimeKind.Unspecified).AddTicks(1107), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GP_001C.r4uset" },
                    { 98, 0, new DateTime(2023, 11, 26, 23, 26, 2, 902, DateTimeKind.Unspecified).AddTicks(1107), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HS_001B.r4uset" },
                    { 99, 0, new DateTime(2023, 11, 26, 23, 26, 2, 902, DateTimeKind.Unspecified).AddTicks(1107), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HS_001T.r4uset" }
                });

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
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 99);

            migrationBuilder.AlterColumn<string>(
                name: "R4UCardSerial",
                table: "R4UCards_Traits",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

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
