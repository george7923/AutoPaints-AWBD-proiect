using LicentaInAngular.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicentaInAngular.Server.DataLayer.Models
{
    public class Useri_Carduri
    {
        [Key]
        public int idUC { get; set; }

        [ForeignKey(nameof(User))]
        public int IdUser { get; set; }
        public User User { get; set; }

        [ForeignKey(nameof(IdCard))] 
        public int IdCard { get; set; }
        public Carduri Card { get; set; }

        [Required]
        public DateTime DataAdaugarii { get; set; }
    }

}

