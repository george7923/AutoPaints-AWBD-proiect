using System.ComponentModel.DataAnnotations;

namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class ComandaSubmitDTO
    {
        public int IdUser { get; set; }
        public int IdAdresa { get; set; }
        public decimal PretTotal { get; set; }
    }
}
