using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicentaInAngular.Server.DataLayer.Models
{
    [Index(nameof(DenumireStrada), IsUnique = true)]
    public class Strazi
    {
        [Key]

        public int IdStrada { get; set; }
        public string DenumireStrada { get; set; }
        public int Nr { get; set; }
        [ForeignKey("Localitati")]
        public int IdLocalitate { get; set; }
        public Localitati Localitati { get; set; }
    }
}
