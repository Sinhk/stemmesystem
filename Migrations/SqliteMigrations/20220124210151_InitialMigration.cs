using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqliteMigrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arrangement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Navn = table.Column<string>(type: "TEXT", nullable: false),
                    Beskrivelse = table.Column<string>(type: "TEXT", nullable: true),
                    Startdato = table.Column<DateTime>(type: "date", nullable: true),
                    Sluttdato = table.Column<DateTime>(type: "date", nullable: true),
                    Aktiv = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arrangement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Delegat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Delegatkode = table.Column<string>(type: "TEXT", nullable: false),
                    Delegatnummer = table.Column<int>(type: "INTEGER", nullable: false),
                    Navn = table.Column<string>(type: "TEXT", nullable: true),
                    Gruppe = table.Column<string>(type: "TEXT", nullable: true),
                    Epost = table.Column<string>(type: "TEXT", nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", nullable: true),
                    ArrangementId = table.Column<int>(type: "INTEGER", nullable: false),
                    SendtSms = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    SendtEmail = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delegat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Delegat_Arrangement_ArrangementId",
                        column: x => x.ArrangementId,
                        principalTable: "Arrangement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sak",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nummer = table.Column<string>(type: "TEXT", nullable: false),
                    Tittel = table.Column<string>(type: "TEXT", nullable: false),
                    Beskrivelse = table.Column<string>(type: "TEXT", nullable: true),
                    ArrangementId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sak", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sak_Arrangement_ArrangementId",
                        column: x => x.ArrangementId,
                        principalTable: "Arrangement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votering",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tittel = table.Column<string>(type: "TEXT", nullable: false),
                    Beskrivelse = table.Column<string>(type: "TEXT", nullable: true),
                    Hemmelig = table.Column<bool>(type: "INTEGER", nullable: false),
                    Aktiv = table.Column<bool>(type: "INTEGER", nullable: false),
                    KanVelge = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTid = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    SluttTid = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    SakId = table.Column<int>(type: "INTEGER", nullable: false),
                    Lukket = table.Column<bool>(type: "INTEGER", nullable: false),
                    Publisert = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votering", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votering_Sak_SakId",
                        column: x => x.SakId,
                        principalTable: "Sak",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DelegatVotering",
                columns: table => new
                {
                    AvgitStemmeId = table.Column<int>(type: "INTEGER", nullable: false),
                    HarStemmtIId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegatVotering", x => new { x.AvgitStemmeId, x.HarStemmtIId });
                    table.ForeignKey(
                        name: "FK_DelegatVotering_Delegat_AvgitStemmeId",
                        column: x => x.AvgitStemmeId,
                        principalTable: "Delegat",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DelegatVotering_Votering_HarStemmtIId",
                        column: x => x.HarStemmtIId,
                        principalTable: "Votering",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stemme",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValgId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DelegatId = table.Column<int>(type: "INTEGER", nullable: true),
                    StemmeHash = table.Column<string>(type: "TEXT", nullable: true),
                    VoteringId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stemme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stemme_Delegat_DelegatId",
                        column: x => x.DelegatId,
                        principalTable: "Delegat",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Stemme_Votering_VoteringId",
                        column: x => x.VoteringId,
                        principalTable: "Votering",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Valg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VoteringId = table.Column<int>(type: "INTEGER", nullable: false),
                    Navn = table.Column<string>(type: "TEXT", nullable: false),
                    SortId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Valg", x => new { x.VoteringId, x.Id });
                    table.ForeignKey(
                        name: "FK_Valg_Votering_VoteringId",
                        column: x => x.VoteringId,
                        principalTable: "Votering",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Delegat_ArrangementId_Delegatnummer",
                table: "Delegat",
                columns: new[] { "ArrangementId", "Delegatnummer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Delegat_Delegatkode",
                table: "Delegat",
                column: "Delegatkode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DelegatVotering_HarStemmtIId",
                table: "DelegatVotering",
                column: "HarStemmtIId");

            migrationBuilder.CreateIndex(
                name: "IX_Sak_ArrangementId",
                table: "Sak",
                column: "ArrangementId");

            migrationBuilder.CreateIndex(
                name: "IX_Stemme_DelegatId",
                table: "Stemme",
                column: "DelegatId");

            migrationBuilder.CreateIndex(
                name: "IX_Stemme_VoteringId",
                table: "Stemme",
                column: "VoteringId");

            migrationBuilder.CreateIndex(
                name: "IX_Votering_SakId",
                table: "Votering",
                column: "SakId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DelegatVotering");

            migrationBuilder.DropTable(
                name: "Stemme");

            migrationBuilder.DropTable(
                name: "Valg");

            migrationBuilder.DropTable(
                name: "Delegat");

            migrationBuilder.DropTable(
                name: "Votering");

            migrationBuilder.DropTable(
                name: "Sak");

            migrationBuilder.DropTable(
                name: "Arrangement");
        }
    }
}
