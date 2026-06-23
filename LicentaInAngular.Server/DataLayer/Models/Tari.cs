using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LicentaInAngular.Server.DataLayer.Models
{
    [Index(nameof(DenumireTara), IsUnique = true)]
    public class Tari
    {
        [Key]
        public int IdTara { get; set; }
        public string DenumireTara { get; set; }
    }
}
