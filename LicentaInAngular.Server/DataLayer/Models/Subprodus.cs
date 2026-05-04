using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LicentaInAngular.Server.Models
{
    public class Subprodus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdSubprodus { get; set; }

        [ForeignKey("Produs")]
        public int IdProdus { get; set; }
        public Produs? Produs { get; set; }

        [Required]
        public bool Valabil { get; set; }

        [ForeignKey("Cos")]
        public int? idCos { get; set; }
        public Cos? Cos { get; set; }
    }
}
