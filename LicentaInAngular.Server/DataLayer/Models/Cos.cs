using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LicentaInAngular.Server.Models
{
    public class Cos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idCos { get; set; }

        [Required]
        public string CodUnic { get; set; }

        [ForeignKey("User")]  // ForeignKey to User (Admin)
        public int IdUser { get; set; }  // Nullable ForeignKey to User (Admin)

        public User? User { get; set; }

        [Required]
        public DateTime DataCreare { get; set; } // To track creation time for automatic deletion
    }
}
