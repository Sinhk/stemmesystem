using Microsoft.EntityFrameworkCore.Migrations;

namespace Stemmesystem.Data.Migrations
{
    public partial class SakNummerToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nummer",
                schema: "stemme",
                table: "Sak",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Nummer",
                schema: "stemme",
                table: "Sak",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
