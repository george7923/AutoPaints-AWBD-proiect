using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicentaInAngular.Server.DataLayer.Models
{


    [Index(nameof(DenumireJudet), IsUnique = true)]
    public class Judete
    {
        [Key]
        public int IdJudet { get; set; }
        public string DenumireJudet { get; set; }
        [ForeignKey("Tari")]
        public int IdTara { get; set; }
        public Tari Tari { get; set; }

    }
}
