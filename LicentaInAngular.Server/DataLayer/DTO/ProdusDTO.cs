namespace LicentaInAngular.Server.DTOs
{
    public class ProdusDTO
    {
        // MODIFICARE: Am adăugat ProdusId pentru identificare
        public int ProdusId { get; set; }

        public string Nume { get; set; }
        public string? Descriere { get; set; }
        // Se afișează "DA" dacă EsteSpray este true, altfel "NU"
        public string EsteSprayText { get; set; }
        public byte[]? Imagine { get; set; }
        public decimal Pret { get; set; }
        public bool Valabil { get; set; }
        public string Categorie { get; set; }
        public string Vendor { get; set; }
    }
}
