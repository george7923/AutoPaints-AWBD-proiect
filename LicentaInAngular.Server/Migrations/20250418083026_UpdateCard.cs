using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicentaInAngular.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adrese_Useri_Adrese_AdreseIdAdresa",
                table: "Adrese_Useri");

            migrationBuilder.DropForeignKey(
                name: "FK_Adrese_Useri_Users_IdUser",
                table: "Adrese_Useri");

            migrationBuilder.DropIndex(
                name: "IX_Adrese_Useri_AdreseIdAdresa",
                table: "Adrese_Useri");

            migrationBuilder.DropColumn(
                name: "AdreseIdAdresa",
                table: "Adrese_Useri");

            migrationBuilder.AlterColumn<string>(
                name: "NumarCard",
                table: "Carduri",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.CreateIndex(
                name: "IX_Adrese_Useri_IdAdresa",
                table: "Adrese_Useri",
                column: "IdAdresa");

            migrationBuilder.AddForeignKey(
                name: "FK_Adrese_Useri_Adrese_IdAdresa",
                table: "Adrese_Useri",
                column: "IdAdresa",
                principalTable: "Adrese",
                principalColumn: "IdAdresa",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Adrese_Useri_Users_IdUser",
                table: "Adrese_Useri",
                column: "IdUser",
                principalTable: "Users",
                principalColumn: "IdUser",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adrese_Useri_Adrese_IdAdresa",
                table: "Adrese_Useri");

            migrationBuilder.DropForeignKey(
                name: "FK_Adrese_Useri_Users_IdUser",
                table: "Adrese_Useri");

            migrationBuilder.DropIndex(
                name: "IX_Adrese_Useri_IdAdresa",
                table: "Adrese_Useri");

            migrationBuilder.AlterColumn<string>(
                name: "NumarCard",
                table: "Carduri",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "AdreseIdAdresa",
                table: "Adrese_Useri",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Adrese_Useri_AdreseIdAdresa",
                table: "Adrese_Useri",
                column: "AdreseIdAdresa");

            migrationBuilder.AddForeignKey(
                name: "FK_Adrese_Useri_Adrese_AdreseIdAdresa",
                table: "Adrese_Useri",
                column: "AdreseIdAdresa",
                principalTable: "Adrese",
                principalColumn: "IdAdresa",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Adrese_Useri_Users_IdUser",
                table: "Adrese_Useri",
                column: "IdUser",
                principalTable: "Users",
                principalColumn: "IdUser",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
