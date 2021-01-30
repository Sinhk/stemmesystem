using Microsoft.EntityFrameworkCore.Migrations;

namespace Stemmesystem.Data.Migrations
{
    public partial class MoreFieldsOnVotering : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Beskrivelse",
                schema: "stemme",
                table: "Votering",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Lukket",
                schema: "stemme",
                table: "Votering",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Publisert",
                schema: "stemme",
                table: "Votering",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Beskrivelse",
                schema: "stemme",
                table: "Votering");

            migrationBuilder.DropColumn(
                name: "Lukket",
                schema: "stemme",
                table: "Votering");

            migrationBuilder.DropColumn(
                name: "Publisert",
                schema: "stemme",
                table: "Votering");
        }
    }
}
