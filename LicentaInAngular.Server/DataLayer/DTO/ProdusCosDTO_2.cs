namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class ProdusCosDTO_2
    {
        public int IdProdus { get; set; }
        public string Nume { get; set; }
        public decimal Pret { get; set; }
        public string Categorie { get; set; }
        public bool EsteSpray { get; set; }
        public string CodCuloare { get; set; }
        public byte[] Imagine { get; set; } // Imaginea vine sub formă de byte[]
        public int Cantitatea { get; set; } // Numărul de subproduse din coș
    }
}
