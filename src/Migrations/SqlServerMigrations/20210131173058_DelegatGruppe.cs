using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlServerMigrations
{
    public partial class DelegatGruppe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "stemme",
                table: "Votering",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gruppe",
                schema: "stemme",
                table: "Delegat");
        }
    }
}
