using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PostgresMigrations
{
    public partial class SendLogFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SendtEmail",
                schema: "stemme",
                table: "Delegat",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SendtSms",
                schema: "stemme",
                table: "Delegat",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendtEmail",
                schema: "stemme",
                table: "Delegat");

            migrationBuilder.DropColumn(
                name: "SendtSms",
                schema: "stemme",
                table: "Delegat");
        }
    }
}
