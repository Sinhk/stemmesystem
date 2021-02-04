using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SqliteMigrations
{
    public partial class SendLogFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SendtEmail",
                table: "Delegat",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SendtSms",
                table: "Delegat",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendtEmail",
                table: "Delegat");

            migrationBuilder.DropColumn(
                name: "SendtSms",
                table: "Delegat");
        }
    }
}
