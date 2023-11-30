using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    /// <inheritdoc />
    public partial class AddsSets20231130 : Migration
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
                    { 113, 0, new DateTime(2023, 11, 30, 18, 29, 58, 665, DateTimeKind.Unspecified).AddTicks(5019), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_NJPW_001P.r4uset" },
                    { 114, 0, new DateTime(2023, 11, 30, 18, 29, 58, 665, DateTimeKind.Unspecified).AddTicks(5019), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_STD_001P.r4uset" },
                    { 115, 0, new DateTime(2023, 11, 30, 18, 29, 58, 665, DateTimeKind.Unspecified).AddTicks(5019), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TOP_001T.r4uset" },
                    { 116, 0, new DateTime(2023, 11, 30, 18, 29, 58, 665, DateTimeKind.Unspecified).AddTicks(5019), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TOP_001B.r4uset" },
                    { 117, 0, new DateTime(2023, 11, 30, 18, 29, 58, 665, DateTimeKind.Unspecified).AddTicks(5019), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TS_001T.r4uset" },
                    { 118, 0, new DateTime(2023, 11, 30, 18, 29, 58, 665, DateTimeKind.Unspecified).AddTicks(5019), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TS_001B.r4uset" }
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
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 118);

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
