using LicentaInAngular.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicentaInAngular.Server.DataLayer.Models
{
    public class Preturi_Produs
    {
        [Key]
        public int idPP { get; set; }
        [ForeignKey("Produs")]
        public int IdProdus { get; set; }
        public Produs Produs { get; set; }
        [Required]
        public decimal Pret { get; set; }
        [Required]
        public DateTime DataInceput { get; set; }
        public DateTime? DataExpirare { get; set; }
        public decimal? Comision { get; set; }
    }
}
