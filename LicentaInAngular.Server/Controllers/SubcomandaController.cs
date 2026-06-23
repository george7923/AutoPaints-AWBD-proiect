using Microsoft.AspNetCore.Mvc;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Models;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcomandaController : ControllerBase
    {
        private readonly SubcomandaService _subcomandaService;

        public SubcomandaController(SubcomandaService subcomandaService)
        {
            _subcomandaService = subcomandaService;
        }

        // GET: api/Subcomanda/by-comanda/{idComanda}
        [HttpGet("by-comanda/{idComanda}")]
        public async Task<IActionResult> GetProdusByComandaId(int idComanda)
        {
            return await _subcomandaService.GetProdusByComandaIdResponse(idComanda);
        }

        // GET: api/Subcomanda
        [HttpGet]
        public async Task<IActionResult> GetAllSubcomenzi()
        {
            return await _subcomandaService.GetAllSubcomenziResponse();
        }

        // GET: api/Subcomanda/{idSubcomanda}
        [HttpGet("{idSubcomanda}")]
        public async Task<IActionResult> GetSubcomandaById(int idSubcomanda)
        {
            return await _subcomandaService.GetSubcomandaByIdResponse(idSubcomanda);
        }

        // GET: api/Subcomanda/all-products
        [HttpGet("all-products")]
        public async Task<IActionResult> GetAllProductsFromSubcomenzi()
        {
            return await _subcomandaService.GetAllProductsFromSubcomenziResponse();
        }

        // GET: api/Subcomanda/by-user/{idUser}
        [HttpGet("by-user/{idUser}")]
        public async Task<IActionResult> GetProdusByUserId(int idUser)
        {
            return await _subcomandaService.GetProdusByUserIdResponse(idUser);
        }

        // POST: api/Subcomanda
        [HttpPost]
        public async Task<IActionResult> CreateSubcomanda([FromBody] Subcomanda subcomanda)
        {
            return await _subcomandaService.CreateSubcomandaResponse(subcomanda);
        }

        // DELETE: api/Subcomanda/{idSubcomanda}
        [HttpDelete("{idSubcomanda}")]
        public async Task<IActionResult> DeleteSubcomanda(int idSubcomanda)
        {
            return await _subcomandaService.DeleteSubcomandaResponse(idSubcomanda);
        }

        // DELETE: api/Subcomanda/delete-subcomanda?idComanda={idComanda}&idProdus={idProdus}
        [HttpDelete("delete-subcomanda")]
        public async Task<IActionResult> DeleteSubcomandaByComandaAndProdus(int idComanda, int idProdus)
        {
            return await _subcomandaService.DeleteSubcomandaByComandaAndProdusResponse(idComanda, idProdus);
        }
    }
}