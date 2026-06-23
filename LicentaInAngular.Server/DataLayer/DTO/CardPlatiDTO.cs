namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class CardPlatiDTO
    {
        public string NumarCard { get; set; }
        public string CVV { get; set; }
        public DateTime DataExpirare { get; set; }
        public string Username { get; set; } // Vom folosi Username pentru a obține UserId
    }


}
