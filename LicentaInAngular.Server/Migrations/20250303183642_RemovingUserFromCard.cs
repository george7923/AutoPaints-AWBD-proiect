using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicentaInAngular.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemovingUserFromCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carduri_Users_IdUser",
                table: "Carduri");

            migrationBuilder.DropIndex(
                name: "IX_Carduri_IdUser",
                table: "Carduri");

            migrationBuilder.DropColumn(
                name: "IdUser",
                table: "Carduri");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdUser",
                table: "Carduri",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carduri_IdUser",
                table: "Carduri",
                column: "IdUser");

            migrationBuilder.AddForeignKey(
                name: "FK_Carduri_Users_IdUser",
                table: "Carduri",
                column: "IdUser",
                principalTable: "Users",
                principalColumn: "IdUser");
        }
    }
}
