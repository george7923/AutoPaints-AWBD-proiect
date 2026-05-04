using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LicentaInAngular.Server.DataLayer.Models;

namespace LicentaInAngular.Server.Models
{
    public class Comanda
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int IdComanda { get; set; }

        [ForeignKey("User")]  // ForeignKey to User (Admin)
        public int IdUser { get; set; }  // Nullable ForeignKey to User (Admin)
        public User? User { get; set; }

        [ForeignKey("Adresa")]
        public int IdAdresa { get; set; }
        public Adresa? Adresa { get; set; }

        public int? IdCard_CC { get; set; }

        [ForeignKey("IdCard_CC")]
        public Carduri Card_CC { get; set; }

        public DateTime? ETA { get; set; }
        [Required]
        public double PretTotal { get; set; }

        [Required]
        public bool IsPlaced { get; set; }
    }
}
