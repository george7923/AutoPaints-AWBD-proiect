using LicentaInAngular.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicentaInAngular.Server.DataLayer.Models
{
    public class Imagini
    {
        [Key]  // Marks the IdImagine as the primary key
        public int idImagine { get; set; }
        [Required]
        public byte[] Fisier { get; set; }  // Nullable byte array for image
        [ForeignKey("Produs")]
        public int IdProdus { get; set; }  // Nullable ForeignKey to Produs
        public Produs Produs { get; set; }  // Nullable navigation property to Produs
    }
}
