using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    public partial class CreateInitialDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MigrationLog",
                columns: table => new
                {
                    LogID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Activity = table.Column<int>(nullable: false),
                    Target = table.Column<string>(nullable: true),
                    IsDone = table.Column<bool>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationLog", x => x.LogID);
                });

            migrationBuilder.CreateTable(
                name: "R4UReleaseSets",
                columns: table => new
                {
                    ReleaseID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReleaseCode = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_R4UReleaseSets", x => x.ReleaseID);
                });

            migrationBuilder.CreateTable(
                name: "R4UCards",
                columns: table => new
                {
                    Serial = table.Column<string>(nullable: false),
                    NonFoilSerial = table.Column<string>(nullable: true),
                    Name_EN = table.Column<string>(nullable: true),
                    Name_JP = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Color = table.Column<int>(nullable: false),
                    Rarity = table.Column<string>(nullable: true),
                    Cost = table.Column<int>(nullable: true),
                    ATK = table.Column<int>(nullable: true),
                    DEF = table.Column<int>(nullable: true),
                    Flavor = table.Column<string>(nullable: true),
                    Effect = table.Column<string>(nullable: true),
                    Images = table.Column<string>(nullable: true),
                    Remarks = table.Column<string>(nullable: true),
                    Language = table.Column<int>(nullable: false),
                    SetReleaseID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Serial", x => x.Serial);
                    table.ForeignKey(
                        name: "FK_R4UCards_R4UCards_NonFoilSerial",
                        column: x => x.NonFoilSerial,
                        principalTable: "R4UCards",
                        principalColumn: "Serial",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_R4UCards_R4UReleaseSets_SetReleaseID",
                        column: x => x.SetReleaseID,
                        principalTable: "R4UReleaseSets",
                        principalColumn: "ReleaseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "R4UCards_Traits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EN = table.Column<string>(nullable: true),
                    JP = table.Column<string>(nullable: true),
                    R4UCardSerial = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_R4UCards_Traits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_R4UCards_Traits_R4UCards_R4UCardSerial",
                        column: x => x.R4UCardSerial,
                        principalTable: "R4UCards",
                        principalColumn: "Serial",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_R4UCards_NonFoilSerial",
                table: "R4UCards",
                column: "NonFoilSerial");

            migrationBuilder.CreateIndex(
                name: "IX_R4UCards_SetReleaseID",
                table: "R4UCards",
                column: "SetReleaseID");

            migrationBuilder.CreateIndex(
                name: "IX_R4UCards_Traits_R4UCardSerial",
                table: "R4UCards_Traits",
                column: "R4UCardSerial");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MigrationLog");

            migrationBuilder.DropTable(
                name: "R4UCards_Traits");

            migrationBuilder.DropTable(
                name: "R4UCards");

            migrationBuilder.DropTable(
                name: "R4UReleaseSets");
        }
    }
}
