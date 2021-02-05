using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PostgresMigrations
{
    public partial class DateOnArrangement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Sluttdato",
                schema: "stemme",
                table: "Arrangement",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Startdato",
                schema: "stemme",
                table: "Arrangement",
                type: "date",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sluttdato",
                schema: "stemme",
                table: "Arrangement");

            migrationBuilder.DropColumn(
                name: "Startdato",
                schema: "stemme",
                table: "Arrangement");
        }
    }
}
