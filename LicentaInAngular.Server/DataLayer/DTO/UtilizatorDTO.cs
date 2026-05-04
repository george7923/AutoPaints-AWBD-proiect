using LicentaInAngular.Server.Models;

namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class UtilizatorDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int? Telefon { get; set; }
        public Adresa Adresa { get; set; }
        public CardPlatiDTO Card { get; set; }

    }






}