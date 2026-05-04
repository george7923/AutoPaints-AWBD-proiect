using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LicentaInAngular.Server.Models
{
    public class Subcomanda
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int IdSubcomanda { get; set; }


        [ForeignKey("Produs")]
        public int IdProdus { get; set; }
        public Produs Produs { get; set; }

        [Required]
        public int TotalSubproduse { get; set; }


        [ForeignKey("Comanda")]
        public int IdComanda { get; set; }
        public Comanda Comanda { get; set; }
    }
}