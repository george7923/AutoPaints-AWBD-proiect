using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LicentaInAngular.Server.DataLayer.Models
{
    [Index(nameof(DenumireCategorie), IsUnique = true)]  // Ensures uniqueness
    public class Categorii
    {
        [Key]
        public int IdCategorie { get; set; }

        [Required]
        [MaxLength(100)]  // Set a reasonable max length
        public string DenumireCategorie { get; set; }  // Unique Category Name

        [Required]
        public string DescriereCategorie { get; set; }
    }
}
