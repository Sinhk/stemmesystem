using Microsoft.EntityFrameworkCore.Migrations;

namespace SqliteMigrations
{
    public partial class DelegatGruppe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Gruppe",
                table: "Delegat",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gruppe",
                table: "Delegat");
        }
    }
}
