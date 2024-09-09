using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StemmeSystem.Data.Migrations
{
    public partial class AddingMemberIdAndImportCheckIn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MinSpeidingOptions_ImportCheckIn",
                schema: "stemme",
                table: "Arrangement",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinSpeidingOptions_ImportCheckIn",
                schema: "stemme",
                table: "Arrangement");
        }
    }
}
