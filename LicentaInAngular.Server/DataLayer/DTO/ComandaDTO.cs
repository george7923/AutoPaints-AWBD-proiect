
using LicentaInAngular.Server.DataLayer.DTO;

public class ComandaDTO
{
    public int IdComanda { get; set; }
    public int IdUser { get; set; }
    public int IdAdresa { get; set; }
    public decimal PretTotal { get; set; }
    public DateTime ETA { get; set; }
    public List<SubcomandaDTO> Subcomenzi { get; set; } = new List<SubcomandaDTO>();
}