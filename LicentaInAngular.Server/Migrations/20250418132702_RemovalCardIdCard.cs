using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicentaInAngular.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemovalCardIdCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Useri_Carduri_Carduri_CardIdCard",
                table: "Useri_Carduri");

            migrationBuilder.DropIndex(
                name: "IX_Useri_Carduri_CardIdCard",
                table: "Useri_Carduri");

            migrationBuilder.DropColumn(
                name: "CardIdCard",
                table: "Useri_Carduri");

            migrationBuilder.CreateIndex(
                name: "IX_Useri_Carduri_IdCard",
                table: "Useri_Carduri",
                column: "IdCard");

            migrationBuilder.AddForeignKey(
                name: "FK_Useri_Carduri_Carduri_IdCard",
                table: "Useri_Carduri",
                column: "IdCard",
                principalTable: "Carduri",
                principalColumn: "IdCard",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Useri_Carduri_Carduri_IdCard",
                table: "Useri_Carduri");

            migrationBuilder.DropIndex(
                name: "IX_Useri_Carduri_IdCard",
                table: "Useri_Carduri");

            migrationBuilder.AddColumn<int>(
                name: "CardIdCard",
                table: "Useri_Carduri",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Useri_Carduri_CardIdCard",
                table: "Useri_Carduri",
                column: "CardIdCard");

            migrationBuilder.AddForeignKey(
                name: "FK_Useri_Carduri_Carduri_CardIdCard",
                table: "Useri_Carduri",
                column: "CardIdCard",
                principalTable: "Carduri",
                principalColumn: "IdCard",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
