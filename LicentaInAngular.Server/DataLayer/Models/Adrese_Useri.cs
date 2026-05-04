using LicentaInAngular.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicentaInAngular.Server.DataLayer.Models
{
    public class Adrese_Useri
    {
        [Key]
        public int idAU { get; set; }
        [ForeignKey("User")]
        public int IdUser { get; set; }
        public User User { get; set; }
        [ForeignKey("Adresa")]
        public int IdAdresa { get; set; }
        public Adresa Adrese { get; set; }
    }
}
