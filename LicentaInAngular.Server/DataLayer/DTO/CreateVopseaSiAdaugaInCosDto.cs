namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class CreateVopseaSiAdaugaInCosDto
    {
        // ID-ul utilizatorului
        public int IdUser { get; set; }

        // TIP: "spray" / "vopsea"
        public string TipVopsea { get; set; }
        public string MarcaMasinii { get; set; }
        public string CodCuloare { get; set; }
        public string Model { get; set; }
        public int An { get; set; }
        public string SerieCaroserie { get; set; }

        // text suplimentar
        public string DetaliiSuplimentare { get; set; }

        // Cantitatea de subproduse pe care o punem DIRECT în coș
        public int Cantitate { get; set; }
    }
}
