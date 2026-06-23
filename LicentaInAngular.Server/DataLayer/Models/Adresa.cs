using LicentaInAngular.Server.DataLayer.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicentaInAngular.Server.Models
{
    public class Adresa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAdresa { get; set; }

        [ForeignKey("Strazi")]
        public int IdStrada { get; set; }
        public Strazi Strazi { get; set; }

        public string? Bloc { get; set; }

        public string? Scara { get; set; }

        public string? Etaj {  get; set; }

        public string? Apartament {  get; set; }
    }
}
