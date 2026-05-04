using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicentaInAngular.Server.Models
{


    [Index(nameof(Email), IsUnique = true)]  // Ensures Email is unique
    [Index(nameof(Telefon), IsUnique = true)]  // Ensures Telefon is unique
    public class Persoana
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int IdPersoana { get; set; }

        [Required]
        public string Nume {  get; set; }

        [Required]
        public string Prenume {  get; set; }

        [Required]
        public string Email { get; set; }

        public string tipPersoana { get; set; } //Fizica/Juridica
        public string? Telefon { get; set; }
        public string Rol { get; set; }

    }
}
