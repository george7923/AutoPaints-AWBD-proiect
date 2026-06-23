using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace LicentaInAngular.Server.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int IdUser { get; set; }

        [Required]


        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [ForeignKey("Persoana")]  // This marks the UserId as the foreign key
        public int IdPersoana { get; set; }

        public Persoana Persoana { get; set; }  // Navigation property for Persoana
    }
}
