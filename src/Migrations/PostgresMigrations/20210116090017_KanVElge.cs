using Microsoft.EntityFrameworkCore.Migrations;

namespace PostgresMigrations
{
    public partial class KanVElge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KanVelge",
                schema: "stemme",
                table: "Votering",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KanVelge",
                schema: "stemme",
                table: "Votering");
        }
    }
}
