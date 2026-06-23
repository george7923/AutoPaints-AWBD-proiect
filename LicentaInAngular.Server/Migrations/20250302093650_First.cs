using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicentaInAngular.Server.Migrations
{
    /// <inheritdoc />
    public partial class First : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorii",
                columns: table => new
                {
                    IdCategorie = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DenumireCategorie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriereCategorie = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorii", x => x.IdCategorie);
                });

            migrationBuilder.CreateTable(
                name: "Persoane",
                columns: table => new
                {
                    IdPersoana = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prenume = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    tipPersoana = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persoane", x => x.IdPersoana);
                });

            migrationBuilder.CreateTable(
                name: "Tari",
                columns: table => new
                {
                    IdTara = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DenumireTara = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tari", x => x.IdTara);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    IdUser = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdPersoana = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.IdUser);
                    table.ForeignKey(
                        name: "FK_Users_Persoane_IdPersoana",
                        column: x => x.IdPersoana,
                        principalTable: "Persoane",
                        principalColumn: "IdPersoana",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Judete",
                columns: table => new
                {
                    IdJudet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DenumireJudet = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdTara = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Judete", x => x.IdJudet);
                    table.ForeignKey(
                        name: "FK_Judete_Tari_IdTara",
                        column: x => x.IdTara,
                        principalTable: "Tari",
                        principalColumn: "IdTara",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carduri",
                columns: table => new
                {
                    IdCard = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumarCard = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CVV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataExpirare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUser = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carduri", x => x.IdCard);
                    table.ForeignKey(
                        name: "FK_Carduri_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser");
                });

            migrationBuilder.CreateTable(
                name: "Cosuri",
                columns: table => new
                {
                    idCos = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodUnic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    DataCreare = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cosuri", x => x.idCos);
                    table.ForeignKey(
                        name: "FK_Cosuri_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    IdProdus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descriere = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EsteSpray = table.Column<bool>(type: "bit", nullable: false),
                    Valabil = table.Column<bool>(type: "bit", nullable: false),
                    IdCategorie = table.Column<int>(type: "int", nullable: false),
                    IdUser = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.IdProdus);
                    table.ForeignKey(
                        name: "FK_Products_Categorii_IdCategorie",
                        column: x => x.IdCategorie,
                        principalTable: "Categorii",
                        principalColumn: "IdCategorie",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser");
                });

            migrationBuilder.CreateTable(
                name: "Localitati",
                columns: table => new
                {
                    IdLocalitate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DenumireLocalitate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdJudet = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localitati", x => x.IdLocalitate);
                    table.ForeignKey(
                        name: "FK_Localitati_Judete_IdJudet",
                        column: x => x.IdJudet,
                        principalTable: "Judete",
                        principalColumn: "IdJudet",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Useri_Carduri",
                columns: table => new
                {
                    idUC = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    IdCard = table.Column<int>(type: "int", nullable: false),
                    CardIdCard = table.Column<int>(type: "int", nullable: false),
                    DataAdaugarii = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Useri_Carduri", x => x.idUC);
                    table.ForeignKey(
                        name: "FK_Useri_Carduri_Carduri_CardIdCard",
                        column: x => x.CardIdCard,
                        principalTable: "Carduri",
                        principalColumn: "IdCard",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Useri_Carduri_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Imagini",
                columns: table => new
                {
                    idImagine = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fisier = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    IdProdus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imagini", x => x.idImagine);
                    table.ForeignKey(
                        name: "FK_Imagini_Products_IdProdus",
                        column: x => x.IdProdus,
                        principalTable: "Products",
                        principalColumn: "IdProdus",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Preturi_Produs",
                columns: table => new
                {
                    idPP = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProdus = table.Column<int>(type: "int", nullable: false),
                    Pret = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataInceput = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataExpirare = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comision = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preturi_Produs", x => x.idPP);
                    table.ForeignKey(
                        name: "FK_Preturi_Produs_Products_IdProdus",
                        column: x => x.IdProdus,
                        principalTable: "Products",
                        principalColumn: "IdProdus",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubProduse",
                columns: table => new
                {
                    IdSubprodus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProdus = table.Column<int>(type: "int", nullable: false),
                    Valabil = table.Column<bool>(type: "bit", nullable: false),
                    idCos = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubProduse", x => x.IdSubprodus);
                    table.ForeignKey(
                        name: "FK_SubProduse_Cosuri_idCos",
                        column: x => x.idCos,
                        principalTable: "Cosuri",
                        principalColumn: "idCos");
                    table.ForeignKey(
                        name: "FK_SubProduse_Products_IdProdus",
                        column: x => x.IdProdus,
                        principalTable: "Products",
                        principalColumn: "IdProdus",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vopsele",
                columns: table => new
                {
                    idVopsea = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipVopsea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MarcaMasinii = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodCuloare = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    An = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerieCaroserie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdProdus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vopsele", x => x.idVopsea);
                    table.ForeignKey(
                        name: "FK_Vopsele_Products_IdProdus",
                        column: x => x.IdProdus,
                        principalTable: "Products",
                        principalColumn: "IdProdus",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Strazi",
                columns: table => new
                {
                    IdStrada = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DenumireStrada = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nr = table.Column<int>(type: "int", nullable: false),
                    IdLocalitate = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strazi", x => x.IdStrada);
                    table.ForeignKey(
                        name: "FK_Strazi_Localitati_IdLocalitate",
                        column: x => x.IdLocalitate,
                        principalTable: "Localitati",
                        principalColumn: "IdLocalitate",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Adrese",
                columns: table => new
                {
                    IdAdresa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdStrada = table.Column<int>(type: "int", nullable: false),
                    Bloc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Scara = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etaj = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Apartament = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adrese", x => x.IdAdresa);
                    table.ForeignKey(
                        name: "FK_Adrese_Strazi_IdStrada",
                        column: x => x.IdStrada,
                        principalTable: "Strazi",
                        principalColumn: "IdStrada",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Adrese_Useri",
                columns: table => new
                {
                    idAU = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    IdAdresa = table.Column<int>(type: "int", nullable: false),
                    AdreseIdAdresa = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adrese_Useri", x => x.idAU);
                    table.ForeignKey(
                        name: "FK_Adrese_Useri_Adrese_AdreseIdAdresa",
                        column: x => x.AdreseIdAdresa,
                        principalTable: "Adrese",
                        principalColumn: "IdAdresa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Adrese_Useri_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comenzi",
                columns: table => new
                {
                    IdComanda = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    IdAdresa = table.Column<int>(type: "int", nullable: false),
                    IdCard = table.Column<int>(type: "int", nullable: false),
                    CardIdCard = table.Column<int>(type: "int", nullable: true),
                    ETA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PretTotal = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comenzi", x => x.IdComanda);
                    table.ForeignKey(
                        name: "FK_Comenzi_Adrese_IdAdresa",
                        column: x => x.IdAdresa,
                        principalTable: "Adrese",
                        principalColumn: "IdAdresa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comenzi_Carduri_CardIdCard",
                        column: x => x.CardIdCard,
                        principalTable: "Carduri",
                        principalColumn: "IdCard");
                    table.ForeignKey(
                        name: "FK_Comenzi_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subcomenzi",
                columns: table => new
                {
                    IdSubcomanda = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProdus = table.Column<int>(type: "int", nullable: false),
                    TotalSubproduse = table.Column<int>(type: "int", nullable: false),
                    IdComanda = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subcomenzi", x => x.IdSubcomanda);
                    table.ForeignKey(
                        name: "FK_Subcomenzi_Comenzi_IdComanda",
                        column: x => x.IdComanda,
                        principalTable: "Comenzi",
                        principalColumn: "IdComanda",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subcomenzi_Products_IdProdus",
                        column: x => x.IdProdus,
                        principalTable: "Products",
                        principalColumn: "IdProdus",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adrese_IdStrada",
                table: "Adrese",
                column: "IdStrada");

            migrationBuilder.CreateIndex(
                name: "IX_Adrese_Useri_AdreseIdAdresa",
                table: "Adrese_Useri",
                column: "AdreseIdAdresa");

            migrationBuilder.CreateIndex(
                name: "IX_Adrese_Useri_IdUser",
                table: "Adrese_Useri",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Carduri_IdUser",
                table: "Carduri",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Carduri_NumarCard",
                table: "Carduri",
                column: "NumarCard",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorii_DenumireCategorie",
                table: "Categorii",
                column: "DenumireCategorie",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comenzi_CardIdCard",
                table: "Comenzi",
                column: "CardIdCard");

            migrationBuilder.CreateIndex(
                name: "IX_Comenzi_IdAdresa",
                table: "Comenzi",
                column: "IdAdresa");

            migrationBuilder.CreateIndex(
                name: "IX_Comenzi_IdUser",
                table: "Comenzi",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Cosuri_IdUser",
                table: "Cosuri",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Imagini_IdProdus",
                table: "Imagini",
                column: "IdProdus");

            migrationBuilder.CreateIndex(
                name: "IX_Judete_DenumireJudet",
                table: "Judete",
                column: "DenumireJudet",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Judete_IdTara",
                table: "Judete",
                column: "IdTara");

            migrationBuilder.CreateIndex(
                name: "IX_Localitati_IdJudet",
                table: "Localitati",
                column: "IdJudet");

            migrationBuilder.CreateIndex(
                name: "IX_Persoane_Email",
                table: "Persoane",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persoane_Telefon",
                table: "Persoane",
                column: "Telefon",
                unique: true,
                filter: "[Telefon] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Preturi_Produs_IdProdus",
                table: "Preturi_Produs",
                column: "IdProdus");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IdCategorie",
                table: "Products",
                column: "IdCategorie");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IdUser",
                table: "Products",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Strazi_DenumireStrada",
                table: "Strazi",
                column: "DenumireStrada",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Strazi_IdLocalitate",
                table: "Strazi",
                column: "IdLocalitate");

            migrationBuilder.CreateIndex(
                name: "IX_Subcomenzi_IdComanda",
                table: "Subcomenzi",
                column: "IdComanda");

            migrationBuilder.CreateIndex(
                name: "IX_Subcomenzi_IdProdus",
                table: "Subcomenzi",
                column: "IdProdus");

            migrationBuilder.CreateIndex(
                name: "IX_SubProduse_idCos",
                table: "SubProduse",
                column: "idCos");

            migrationBuilder.CreateIndex(
                name: "IX_SubProduse_IdProdus",
                table: "SubProduse",
                column: "IdProdus");

            migrationBuilder.CreateIndex(
                name: "IX_Tari_DenumireTara",
                table: "Tari",
                column: "DenumireTara",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Useri_Carduri_CardIdCard",
                table: "Useri_Carduri",
                column: "CardIdCard");

            migrationBuilder.CreateIndex(
                name: "IX_Useri_Carduri_IdUser",
                table: "Useri_Carduri",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdPersoana",
                table: "Users",
                column: "IdPersoana");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vopsele_IdProdus",
                table: "Vopsele",
                column: "IdProdus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adrese_Useri");

            migrationBuilder.DropTable(
                name: "Imagini");

            migrationBuilder.DropTable(
                name: "Preturi_Produs");

            migrationBuilder.DropTable(
                name: "Subcomenzi");

            migrationBuilder.DropTable(
                name: "SubProduse");

            migrationBuilder.DropTable(
                name: "Useri_Carduri");

            migrationBuilder.DropTable(
                name: "Vopsele");

            migrationBuilder.DropTable(
                name: "Comenzi");

            migrationBuilder.DropTable(
                name: "Cosuri");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Adrese");

            migrationBuilder.DropTable(
                name: "Carduri");

            migrationBuilder.DropTable(
                name: "Categorii");

            migrationBuilder.DropTable(
                name: "Strazi");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Localitati");

            migrationBuilder.DropTable(
                name: "Persoane");

            migrationBuilder.DropTable(
                name: "Judete");

            migrationBuilder.DropTable(
                name: "Tari");
        }
    }
}
