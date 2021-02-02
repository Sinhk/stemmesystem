using Microsoft.EntityFrameworkCore.Migrations;

namespace SqliteMigrations
{
    public partial class OptionalDelegatIdOnStemme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stemme_Delegat_DelegatId",
                table: "Stemme");

            migrationBuilder.AlterColumn<int>(
                name: "DelegatId",
                table: "Stemme",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Stemme_Delegat_DelegatId",
                table: "Stemme",
                column: "DelegatId",
                principalTable: "Delegat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stemme_Delegat_DelegatId",
                table: "Stemme");

            migrationBuilder.AlterColumn<int>(
                name: "DelegatId",
                table: "Stemme",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Stemme_Delegat_DelegatId",
                table: "Stemme",
                column: "DelegatId",
                principalTable: "Delegat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
