using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicentaInAngular.Server.DataLayer.Models
{
    public class Localitati
    {
        [Key]
        public int IdLocalitate { get; set; }
        public string DenumireLocalitate { get; set; }
        [ForeignKey("Judete")]
        public int IdJudet { get; set; }
        public Judete Judete { get; set; }
    }
}
