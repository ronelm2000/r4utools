using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class AddsSetDeleteCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_R4UCards_R4UCards_NonFoilSerial",
                table: "R4UCards");

            migrationBuilder.DropForeignKey(
                name: "FK_R4UCards_R4UReleaseSets_SetReleaseID",
                table: "R4UCards");

            migrationBuilder.AddForeignKey(
                name: "FK_R4UCards_R4UCards_NonFoilSerial",
                table: "R4UCards",
                column: "NonFoilSerial",
                principalTable: "R4UCards",
                principalColumn: "Serial",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_R4UCards_R4UReleaseSets_SetReleaseID",
                table: "R4UCards",
                column: "SetReleaseID",
                principalTable: "R4UReleaseSets",
                principalColumn: "ReleaseID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_R4UCards_R4UCards_NonFoilSerial",
                table: "R4UCards");

            migrationBuilder.DropForeignKey(
                name: "FK_R4UCards_R4UReleaseSets_SetReleaseID",
                table: "R4UCards");

            migrationBuilder.AddForeignKey(
                name: "FK_R4UCards_R4UCards_NonFoilSerial",
                table: "R4UCards",
                column: "NonFoilSerial",
                principalTable: "R4UCards",
                principalColumn: "Serial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_R4UCards_R4UReleaseSets_SetReleaseID",
                table: "R4UCards",
                column: "SetReleaseID",
                principalTable: "R4UReleaseSets",
                principalColumn: "ReleaseID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
