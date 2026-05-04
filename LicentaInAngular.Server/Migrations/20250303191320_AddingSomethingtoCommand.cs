using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicentaInAngular.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddingSomethingtoCommand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comenzi_Carduri_CardIdCard",
                table: "Comenzi");

            migrationBuilder.RenameColumn(
                name: "IdCard",
                table: "Comenzi",
                newName: "IdCard_CC");

            migrationBuilder.RenameColumn(
                name: "CardIdCard",
                table: "Comenzi",
                newName: "Card_CCidUC");

            migrationBuilder.RenameIndex(
                name: "IX_Comenzi_CardIdCard",
                table: "Comenzi",
                newName: "IX_Comenzi_Card_CCidUC");

            migrationBuilder.AddForeignKey(
                name: "FK_Comenzi_Useri_Carduri_Card_CCidUC",
                table: "Comenzi",
                column: "Card_CCidUC",
                principalTable: "Useri_Carduri",
                principalColumn: "idUC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comenzi_Useri_Carduri_Card_CCidUC",
                table: "Comenzi");

            migrationBuilder.RenameColumn(
                name: "IdCard_CC",
                table: "Comenzi",
                newName: "IdCard");

            migrationBuilder.RenameColumn(
                name: "Card_CCidUC",
                table: "Comenzi",
                newName: "CardIdCard");

            migrationBuilder.RenameIndex(
                name: "IX_Comenzi_Card_CCidUC",
                table: "Comenzi",
                newName: "IX_Comenzi_CardIdCard");

            migrationBuilder.AddForeignKey(
                name: "FK_Comenzi_Carduri_CardIdCard",
                table: "Comenzi",
                column: "CardIdCard",
                principalTable: "Carduri",
                principalColumn: "IdCard");
        }
    }
}
