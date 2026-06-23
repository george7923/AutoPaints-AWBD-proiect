using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/comanda")]
    [ApiController]
    public class ComandaController : ControllerBase
    {
        private readonly ComandaService _comandaService;

        public ComandaController(ComandaService comandaService)
        {
            _comandaService = comandaService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetComandaById(int id)
        {
            return await _comandaService.GetComandaByIdResponse(id);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllComenzi()
        {
            return await _comandaService.GetAllComenziResponse();
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetComenziByUserId(int userId)
        {
            return await _comandaService.GetComenziByUserIdResponse(userId);
        }

        [HttpPost("submit-with-payment")]
        public async Task<IActionResult> SubmitComandaWithPayment([FromBody] ComandaPaymentDTO dto)
        {
            return await _comandaService.SubmitComandaWithPaymentResponse(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComanda(int id, [FromBody] Comanda comanda)
        {
            return await _comandaService.UpdateComandaResponse(id, comanda);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComanda(int id)
        {
            return await _comandaService.DeleteComandaResponse(id);
        }

        [HttpPost("emitere")]
        public async Task<IActionResult> EmitereComanda([FromBody] ComandaEmitereDTO dto)
        {
            return await _comandaService.EmitereComandaResponse(dto, ModelState.IsValid);
        }

        [HttpPut("livrare/{id}")]
        public async Task<IActionResult> MarcheazaCaLivrataSauNelivrata(int id, [FromBody] LivrareDto dto)
        {
            return await _comandaService.MarcheazaCaLivrataSauNelivrataResponse(id, dto);
        }

        [HttpGet("afisare/{userId}")]
        public async Task<IActionResult> GetToateComenzileAleUtilizatorului(int userId)
        {
            return await _comandaService.GetToateComenzileAleUtilizatoruluiResponse(userId);
        }

        [HttpPost("emitere-cash")]
        public async Task<IActionResult> EmitereComandaCash([FromBody] ComandaEmitereDTO dto)
        {
            return await _comandaService.EmitereComandaCashResponse(dto, ModelState.IsValid);
        }

        [HttpGet("scurt/user/{userId}")]
        public async Task<IActionResult> GetComenziScurtByUser(int userId)
        {
            return await _comandaService.GetComenziScurtByUserResponse(userId);
        }

        [HttpGet("generare-pdf/{idComanda}")]
        public async Task<IActionResult> GenerareBonFiscal(int idComanda)
        {
            return await _comandaService.GenerareBonFiscalResponse(idComanda);
        }
    }

    public class LivrareDto
    {
        public bool Livrata { get; set; }
    }
}

public class ProdusComandaBonFiscalDTO
{
    public string NumeProdus { get; set; }
    public int Cantitate { get; set; }
    public double PretUnitar { get; set; }
}

public class BonFiscalDTO
{
    public int IdComanda { get; set; }
    public DateTime ETA { get; set; }
    public double PretTotal { get; set; }
    public bool IsPlaced { get; set; }

    public string Tara { get; set; }
    public string Judet { get; set; }
    public string Localitate { get; set; }
    public string Strada { get; set; }
    public int Nr { get; set; }
    public string? Bloc { get; set; }
    public string? Scara { get; set; }
    public string? Etaj { get; set; }
    public string? Apartament { get; set; }

    public List<ProdusComandaBonFiscalDTO> Produse { get; set; } = new();
}