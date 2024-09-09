using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stemmesystem.Data.Migrations
{
    public partial class MinSpeidingOptionsApiKeyLimit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MinSpeidingOptions_MembersApiKey",
                schema: "stemme",
                table: "Arrangement",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MinSpeidingOptions_MembersApiKey",
                schema: "stemme",
                table: "Arrangement",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
