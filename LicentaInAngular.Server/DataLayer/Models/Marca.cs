
using System.ComponentModel.DataAnnotations;
namespace LicentaInAngular.Server.DataLayer.Models 
{ 
    public class Marca
    {
        [Key]
        public int IdMarca { get; set; }
        public string NumeMarca { get; set; }

        public ICollection<Model> Modele { get; set; }
    }
}