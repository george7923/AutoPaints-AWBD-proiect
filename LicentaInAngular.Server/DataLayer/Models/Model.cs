using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LicentaInAngular.Server.DataLayer.Models
{
    public class Model
    {
        [Key]
        public int IdModel { get; set; }

        public string NumeModel { get; set; }
        public int An { get; set; }

        // FK către Marca
        public int IdMarca { get; set; }
        [ForeignKey("IdMarca")]
        public Marca Marca { get; set; }

        public ICollection<Vopsea> Vopsele { get; set; }
    }

}
