using LicentaInAngular.Server.DataLayer.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicentaInAngular.Server.Models
{
    public class Produs
    {
        [Key] 
        public int IdProdus { get; set; }

        [Required]  
        
        public string Nume { get; set; }

        public string? Descriere { get; set; } 

        [Required]
        public bool EsteSpray { get; set; } 
        [Required]
        public bool Valabil { get; set; } 


        [ForeignKey("Categorii")]  
        public int IdCategorie { get; set; }  
        public Categorii Categorii { get; set; }  

        [ForeignKey("User")]  
        public int? IdUser { get; set; }  

        public User? User { get; set; }
    }
}
