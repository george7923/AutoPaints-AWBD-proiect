namespace LicentaInAngular.Server.DataLayer.DTO
{

    public class ProductUploadDto
    {
        public int IdProdus { get; set; }
        public string Nume { get; set; }
        public string? Descriere { get; set; }
        public bool EsteSpray { get; set; }
        public string? CodCuloare { get; set; }
        public IFormFile? Imagine { get; set; }
        public decimal Pret { get; set; }
        public bool Valabil { get; set; }
        public string Categorie { get; set; }
        public int? IdUser { get; set; }
    }

}
