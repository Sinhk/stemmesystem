using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StemmeSystem.Data.Migrations
{
    public partial class LagreTilstedeVedAvsluttetVotering : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DelegaterTilstede",
                schema: "stemme",
                table: "Votering",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelegaterTilstede",
                schema: "stemme",
                table: "Votering");
        }
    }
}
