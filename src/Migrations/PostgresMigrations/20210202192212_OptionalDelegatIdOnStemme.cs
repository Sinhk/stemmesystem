using Microsoft.EntityFrameworkCore.Migrations;

namespace PostgresMigrations
{
    public partial class OptionalDelegatIdOnStemme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stemme_Delegat_DelegatId",
                schema: "stemme",
                table: "Stemme");

            migrationBuilder.AlterColumn<int>(
                name: "DelegatId",
                schema: "stemme",
                table: "Stemme",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Stemme_Delegat_DelegatId",
                schema: "stemme",
                table: "Stemme",
                column: "DelegatId",
                principalSchema: "stemme",
                principalTable: "Delegat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stemme_Delegat_DelegatId",
                schema: "stemme",
                table: "Stemme");

            migrationBuilder.AlterColumn<int>(
                name: "DelegatId",
                schema: "stemme",
                table: "Stemme",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Stemme_Delegat_DelegatId",
                schema: "stemme",
                table: "Stemme",
                column: "DelegatId",
                principalSchema: "stemme",
                principalTable: "Delegat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
