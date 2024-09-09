using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stemmesystem.Data.Migrations
{
    public partial class MinSpeidingOptionsOnArrangement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MinSpeidingOptions_MembersApiKey",
                schema: "stemme",
                table: "Arrangement",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinSpeidingOptions_MinSpeidingId",
                schema: "stemme",
                table: "Arrangement",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinSpeidingOptions_MembersApiKey",
                schema: "stemme",
                table: "Arrangement");

            migrationBuilder.DropColumn(
                name: "MinSpeidingOptions_MinSpeidingId",
                schema: "stemme",
                table: "Arrangement");
        }
    }
}
