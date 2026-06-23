namespace LicentaInAngular.Server.Utils
{
    public class CardObject
    {
        public string NumarCard { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;
        public string DataExpirare { get; set; } = string.Empty; // in format ISO sau MM/YY
    }

}
