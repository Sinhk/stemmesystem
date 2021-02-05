using Microsoft.EntityFrameworkCore.Migrations;

namespace PostgresMigrations
{
    public partial class NullBeskrivelse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DelegatVotering_Delegat_AvgitStemmeId",
                schema: "stemme",
                table: "DelegatVotering");

            migrationBuilder.AlterColumn<string>(
                name: "Beskrivelse",
                schema: "stemme",
                table: "Votering",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_DelegatVotering_Delegat_AvgitStemmeId",
                schema: "stemme",
                table: "DelegatVotering",
                column: "AvgitStemmeId",
                principalSchema: "stemme",
                principalTable: "Delegat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DelegatVotering_Delegat_AvgitStemmeId",
                schema: "stemme",
                table: "DelegatVotering");

            migrationBuilder.AlterColumn<string>(
                name: "Beskrivelse",
                schema: "stemme",
                table: "Votering",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DelegatVotering_Delegat_AvgitStemmeId",
                schema: "stemme",
                table: "DelegatVotering",
                column: "AvgitStemmeId",
                principalSchema: "stemme",
                principalTable: "Delegat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
