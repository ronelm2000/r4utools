using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    /// <inheritdoc />
    public partial class AddsLycoRecoSSandKS2 : Migration
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
                    { 119, 0, new DateTime(2023, 11, 30, 18, 29, 58, 665, DateTimeKind.Unspecified).AddTicks(5019), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_BA_002B.r4uset" },
                    { 120, 0, new DateTime(2023, 11, 30, 18, 29, 58, 665, DateTimeKind.Unspecified).AddTicks(5019), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GU_002B.r4uset" },
                    { 121, 0, new DateTime(2023, 11, 30, 18, 29, 58, 665, DateTimeKind.Unspecified).AddTicks(5019), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_LR_001E.r4uset" },
                    { 122, 0, new DateTime(2023, 11, 30, 18, 29, 58, 665, DateTimeKind.Unspecified).AddTicks(5019), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KS_002B.r4uset" },
                    { 123, 0, new DateTime(2023, 11, 30, 18, 29, 58, 665, DateTimeKind.Unspecified).AddTicks(5019), false, "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KS_002T.r4uset" }
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
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 123);

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
