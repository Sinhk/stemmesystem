using Microsoft.EntityFrameworkCore.Migrations;

namespace PostgresMigrations
{
    public partial class ChangedCasing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DelegatKode",
                schema: "stemme",
                table: "Delegat",
                newName: "Delegatkode");

            migrationBuilder.CreateIndex(
                name: "IX_Delegat_Delegatkode",
                schema: "stemme",
                table: "Delegat",
                column: "Delegatkode",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Delegat_Delegatkode",
                schema: "stemme",
                table: "Delegat");

            migrationBuilder.RenameColumn(
                name: "Delegatkode",
                schema: "stemme",
                table: "Delegat",
                newName: "DelegatKode");
        }
    }
}
