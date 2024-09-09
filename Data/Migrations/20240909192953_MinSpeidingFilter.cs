using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StemmeSystem.Data.Migrations
{
    public partial class MinSpeidingFilter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                schema: "stemme",
                table: "Delegat",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MinSpeidingOptions_Filter",
                schema: "stemme",
                table: "Arrangement",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberId",
                schema: "stemme",
                table: "Delegat");

            migrationBuilder.DropColumn(
                name: "MinSpeidingOptions_Filter",
                schema: "stemme",
                table: "Arrangement");
        }
    }
}
