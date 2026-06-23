namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class CreateVopseaDto
    {
        // TIP: "spray" sau "vopsea"
        public string TipVopsea { get; set; }

        public string MarcaMasinii { get; set; }
        public string CodCuloare { get; set; }
        public string Model { get; set; }
        public string An { get; set; }
        public string SerieCaroserie { get; set; }

        // Textul introdus în textbox mare
        public string DetaliiSuplimentare { get; set; }

        // Cantitatea de subproduse de generat
        public int Cantitate { get; set; }
    }
}
