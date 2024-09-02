using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stemmesystem.Data.Migrations
{
    public partial class DelegatTilstede : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TilStede",
                schema: "stemme",
                table: "Delegat",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TilStede",
                schema: "stemme",
                table: "Delegat");
        }
    }
}
