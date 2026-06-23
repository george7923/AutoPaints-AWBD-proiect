namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class ComandaCuDetaliiDTO
    {
        public int IdComanda { get; set; }
        public DateTime? ETA { get; set; }
        public bool IsPlaced { get; set; }
        public double PretTotal { get; set; }

        // 📦 Detalii adresă completă
        public string? Tara { get; set; }
        public string? Judet { get; set; }
        public string? Localitate { get; set; }
        public string? Strada { get; set; }
        public int? Nr { get; set; }
        public string? Bloc { get; set; }
        public string? Scara { get; set; }
        public string? Etaj { get; set; }
        public string? Apartament { get; set; }

        // 🛒 Produsele din comandă
        public List<ProdusComandaDTO> Produse { get; set; }
    }


}
