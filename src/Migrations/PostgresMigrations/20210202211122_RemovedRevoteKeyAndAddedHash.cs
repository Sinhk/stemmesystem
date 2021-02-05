using Microsoft.EntityFrameworkCore.Migrations;

namespace PostgresMigrations
{
    public partial class RemovedRevoteKeyAndAddedHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RevoteKey",
                schema: "stemme",
                table: "Stemme",
                newName: "StemmeHash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StemmeHash",
                schema: "stemme",
                table: "Stemme",
                newName: "RevoteKey");
        }
    }
}
