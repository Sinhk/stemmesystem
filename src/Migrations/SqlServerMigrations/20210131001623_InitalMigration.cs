using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlServerMigrations
{
    public partial class InitalMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "stemme");

            migrationBuilder.CreateTable(
                name: "Arrangement",
                schema: "stemme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Navn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beskrivelse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Startdato = table.Column<DateTime>(type: "date", nullable: true),
                    Sluttdato = table.Column<DateTime>(type: "date", nullable: true),
                    Aktiv = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arrangement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "stemme",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "stemme",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                schema: "stemme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Delegat",
                schema: "stemme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Delegatkode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Delegatnummer = table.Column<int>(type: "int", nullable: false),
                    Navn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Epost = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArrangementId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delegat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Delegat_Arrangement_ArrangementId",
                        column: x => x.ArrangementId,
                        principalSchema: "stemme",
                        principalTable: "Arrangement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sak",
                schema: "stemme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nummer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tittel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beskrivelse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArrangementId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sak", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sak_Arrangement_ArrangementId",
                        column: x => x.ArrangementId,
                        principalSchema: "stemme",
                        principalTable: "Arrangement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "stemme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "stemme",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "stemme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "stemme",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "stemme",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "stemme",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "stemme",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "stemme",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "stemme",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "stemme",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "stemme",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votering",
                schema: "stemme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tittel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beskrivelse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hemmelig = table.Column<bool>(type: "bit", nullable: false),
                    Aktiv = table.Column<bool>(type: "bit", nullable: false),
                    KanVelge = table.Column<int>(type: "int", nullable: false),
                    StartTid = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SluttTid = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SakId = table.Column<int>(type: "int", nullable: false),
                    Lukket = table.Column<bool>(type: "bit", nullable: false),
                    Publisert = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votering", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votering_Sak_SakId",
                        column: x => x.SakId,
                        principalSchema: "stemme",
                        principalTable: "Sak",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DelegatVotering",
                schema: "stemme",
                columns: table => new
                {
                    AvgitStemmeId = table.Column<int>(type: "int", nullable: false),
                    HarStemmtIId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegatVotering", x => new { x.AvgitStemmeId, x.HarStemmtIId });
                    table.ForeignKey(
                        name: "FK_DelegatVotering_Delegat_AvgitStemmeId",
                        column: x => x.AvgitStemmeId,
                        principalSchema: "stemme",
                        principalTable: "Delegat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DelegatVotering_Votering_HarStemmtIId",
                        column: x => x.HarStemmtIId,
                        principalSchema: "stemme",
                        principalTable: "Votering",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Stemme",
                schema: "stemme",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValgId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DelegatId = table.Column<int>(type: "int", nullable: false),
                    RevoteKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoteringId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stemme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stemme_Delegat_DelegatId",
                        column: x => x.DelegatId,
                        principalSchema: "stemme",
                        principalTable: "Delegat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Stemme_Votering_VoteringId",
                        column: x => x.VoteringId,
                        principalSchema: "stemme",
                        principalTable: "Votering",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Valg",
                schema: "stemme",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoteringId = table.Column<int>(type: "int", nullable: false),
                    Navn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Valg", x => new { x.VoteringId, x.Id });
                    table.ForeignKey(
                        name: "FK_Valg_Votering_VoteringId",
                        column: x => x.VoteringId,
                        principalSchema: "stemme",
                        principalTable: "Votering",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "stemme",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "stemme",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "stemme",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "stemme",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "stemme",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "stemme",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "stemme",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Delegat_ArrangementId_Delegatnummer",
                schema: "stemme",
                table: "Delegat",
                columns: new[] { "ArrangementId", "Delegatnummer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Delegat_Delegatkode",
                schema: "stemme",
                table: "Delegat",
                column: "Delegatkode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DelegatVotering_HarStemmtIId",
                schema: "stemme",
                table: "DelegatVotering",
                column: "HarStemmtIId");

            migrationBuilder.CreateIndex(
                name: "IX_Sak_ArrangementId",
                schema: "stemme",
                table: "Sak",
                column: "ArrangementId");

            migrationBuilder.CreateIndex(
                name: "IX_Stemme_DelegatId",
                schema: "stemme",
                table: "Stemme",
                column: "DelegatId");

            migrationBuilder.CreateIndex(
                name: "IX_Stemme_VoteringId",
                schema: "stemme",
                table: "Stemme",
                column: "VoteringId");

            migrationBuilder.CreateIndex(
                name: "IX_Votering_SakId",
                schema: "stemme",
                table: "Votering",
                column: "SakId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "DelegatVotering",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "Stemme",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "Valg",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "Delegat",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "Votering",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "Sak",
                schema: "stemme");

            migrationBuilder.DropTable(
                name: "Arrangement",
                schema: "stemme");
        }
    }
}
