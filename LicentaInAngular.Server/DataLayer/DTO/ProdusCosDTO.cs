namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class ProdusCosDTO
    {
        public int IdSubprodus { get; set; }
        public int IdProdus { get; set; }
        public string Nume { get; set; } = "N/A";
        public decimal Pret { get; set; }
        public string Categorie { get; set; } = "Necunoscut";
        public bool EsteSpray { get; set; }
        public string? CodCuloare { get; set; }
        public byte[]? Imagine { get; set; } // 🔹 Adăugat Imagine
        public int cantitatea { get; internal set; }
    }


}
