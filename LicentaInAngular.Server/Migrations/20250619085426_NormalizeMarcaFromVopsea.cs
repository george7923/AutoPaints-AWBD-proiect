using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicentaInAngular.Server.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeMarcaFromVopsea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comenzi_Useri_Carduri_Card_CCidUC",
                table: "Comenzi");

            migrationBuilder.DropColumn(
                name: "MarcaMasinii",
                table: "Vopsele");

            migrationBuilder.RenameColumn(
                name: "Card_CCidUC",
                table: "Comenzi",
                newName: "Card_CCIdCard");

            migrationBuilder.RenameIndex(
                name: "IX_Comenzi_Card_CCidUC",
                table: "Comenzi",
                newName: "IX_Comenzi_Card_CCIdCard");

            migrationBuilder.AddColumn<int>(
                name: "IdMarca",
                table: "Vopsele",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "IdCard_CC",
                table: "Comenzi",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Marci",
                columns: table => new
                {
                    IdMarca = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeMarca = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marci", x => x.IdMarca);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vopsele_IdMarca",
                table: "Vopsele",
                column: "IdMarca");

            migrationBuilder.AddForeignKey(
                name: "FK_Comenzi_Carduri_Card_CCIdCard",
                table: "Comenzi",
                column: "Card_CCIdCard",
                principalTable: "Carduri",
                principalColumn: "IdCard");

            migrationBuilder.AddForeignKey(
                name: "FK_Vopsele_Marci_IdMarca",
                table: "Vopsele",
                column: "IdMarca",
                principalTable: "Marci",
                principalColumn: "IdMarca",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comenzi_Carduri_Card_CCIdCard",
                table: "Comenzi");

            migrationBuilder.DropForeignKey(
                name: "FK_Vopsele_Marci_IdMarca",
                table: "Vopsele");

            migrationBuilder.DropTable(
                name: "Marci");

            migrationBuilder.DropIndex(
                name: "IX_Vopsele_IdMarca",
                table: "Vopsele");

            migrationBuilder.DropColumn(
                name: "IdMarca",
                table: "Vopsele");

            migrationBuilder.RenameColumn(
                name: "Card_CCIdCard",
                table: "Comenzi",
                newName: "Card_CCidUC");

            migrationBuilder.RenameIndex(
                name: "IX_Comenzi_Card_CCIdCard",
                table: "Comenzi",
                newName: "IX_Comenzi_Card_CCidUC");

            migrationBuilder.AddColumn<string>(
                name: "MarcaMasinii",
                table: "Vopsele",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "IdCard_CC",
                table: "Comenzi",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comenzi_Useri_Carduri_Card_CCidUC",
                table: "Comenzi",
                column: "Card_CCidUC",
                principalTable: "Useri_Carduri",
                principalColumn: "idUC");
        }
    }
}
