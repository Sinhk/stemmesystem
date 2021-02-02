using Microsoft.EntityFrameworkCore.Migrations;

namespace SqliteMigrations
{
    public partial class RemovedRevoteKeyAndAddedHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RevoteKey",
                table: "Stemme",
                newName: "StemmeHash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StemmeHash",
                table: "Stemme",
                newName: "RevoteKey");
        }
    }
}
