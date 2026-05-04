using LicentaInAngular.Server.Models;

namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class SablonProdusDTO
    {
        public int IdProdus { get; set; }

        public string Nume { get; set; }

        public string? Descriere { get; set; }

        public bool EsteSpray { get; set; }

        // Imagine preluată din entitatea Imagini, din coloana Fisier
        public byte[]? Imagine { get; set; }

        // Pret preluat din entitatea Preturi_Produs, din coloana Pret
        public decimal Pret { get; set; }

        public bool Valabil { get; set; }

        // Categorie preluată din entitatea Categorii, din coloana DenumireCategorie
        public string Categorie { get; set; }
        public string CodCuloare { get; set; }

        public int? IdUser { get; set; }

        public User? User { get; set; }
    }
}
