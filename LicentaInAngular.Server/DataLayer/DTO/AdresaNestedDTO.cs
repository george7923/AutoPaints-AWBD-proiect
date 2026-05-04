namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class AdresaNestedDTO
    {
        public StraziDTO Strazi { get; set; }
        public string? Bloc { get; set; }
        public string? Scara { get; set; }
        public string? Etaj { get; set; }
        public string? Apartament { get; set; }
    }
}
