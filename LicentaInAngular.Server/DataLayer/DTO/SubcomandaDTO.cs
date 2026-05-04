using System.ComponentModel.DataAnnotations;

namespace LicentaInAngular.Server.DataLayer.DTO
{
    public class SubcomandaDTO
    {
        public int IdSubcomanda { get; set; }
        public int IdProdus { get; set; }
        public int TotalSubproduse { get; set; }
    }
}
