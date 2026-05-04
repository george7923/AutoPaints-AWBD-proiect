using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Vopsea
{
    [Key]
    public int idVopsea { get; set; }

    public string TipVopsea { get; set; }
    public string CodCuloare { get; set; }
    public string SerieCaroserie { get; set; }

    public int IdProdus { get; set; }
    [ForeignKey("IdProdus")]
    public Produs Produs { get; set; }

    // FK către Model
    public int IdModel { get; set; }
    [ForeignKey("IdModel")]
    public Model Model { get; set; }
}

