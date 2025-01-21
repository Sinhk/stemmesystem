using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StemmeSystem.Data.Migrations
{
    public partial class RemovedHemmelig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hemmelig",
                schema: "stemme",
                table: "Votering");

            migrationBuilder.DropColumn(
                name: "StemmeHash",
                schema: "stemme",
                table: "Stemme");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Hemmelig",
                schema: "stemme",
                table: "Votering",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StemmeHash",
                schema: "stemme",
                table: "Stemme",
                type: "text",
                nullable: true);
        }
    }
}
