using Microsoft.AspNetCore.Mvc;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Models;
using System.Threading.Tasks;
using System.Linq;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcomandaController : ControllerBase
    {
        private readonly ISubcomandaRepository _subcomandaRepository;

        public SubcomandaController(ISubcomandaRepository subcomandaRepository)
        {
            _subcomandaRepository = subcomandaRepository;
        }

        // GET: api/Subcomanda/by-comanda/{idComanda}
        [HttpGet("by-comanda/{idComanda}")]
        public async Task<IActionResult> GetProdusByComandaId(int idComanda)
        {
            var produse = await _subcomandaRepository.GetProdusByComandaId(idComanda);
            if (produse == null || !produse.Any())
            {
                return NotFound(new { message = "No products found for this order." });
            }
            return Ok(produse);
        }

        // GET: api/Subcomanda
        [HttpGet]
        public async Task<IActionResult> GetAllSubcomenzi()
        {
            var subcomenzi = await _subcomandaRepository.GetAllSubcomenzi();
            return Ok(subcomenzi);
        }

        // GET: api/Subcomanda/{idSubcomanda}
        [HttpGet("{idSubcomanda}")]
        public async Task<IActionResult> GetSubcomandaById(int idSubcomanda)
        {
            var subcomanda = await _subcomandaRepository.GetSubcomandaById(idSubcomanda);
            if (subcomanda == null)
            {
                return NotFound(new { message = "Suborder not found." });
            }
            return Ok(subcomanda);
        }

        // GET: api/Subcomanda/all-products
        [HttpGet("all-products")]
        public async Task<IActionResult> GetAllProductsFromSubcomenzi()
        {
            var produse = await _subcomandaRepository.GetAllProductsFromSubcomenzi();
            if (produse == null || !produse.Any())
            {
                return NotFound(new { message = "No products found in any suborder." });
            }
            return Ok(produse);
        }

        // GET: api/Subcomanda/by-user/{idUser}
        [HttpGet("by-user/{idUser}")]
        public async Task<IActionResult> GetProdusByUserId(int idUser)
        {
            var produse = await _subcomandaRepository.GetProdusByUserId(idUser);
            if (produse == null || !produse.Any())
            {
                return NotFound(new { message = "No products found for this user." });
            }
            return Ok(produse);
        }

        // POST: api/Subcomanda
        [HttpPost]
        public async Task<IActionResult> CreateSubcomanda([FromBody] Subcomanda subcomanda)
        {
            if (subcomanda == null)
            {
                return BadRequest(new { message = "Suborder data is required." });
            }
            await _subcomandaRepository.AddSubcomanda(subcomanda);
            return CreatedAtAction(nameof(GetSubcomandaById), new { idSubcomanda = subcomanda.IdSubcomanda }, subcomanda);
        }

        // DELETE: api/Subcomanda/{idSubcomanda}
        [HttpDelete("{idSubcomanda}")]
        public async Task<IActionResult> DeleteSubcomanda(int idSubcomanda)
        {
            await _subcomandaRepository.DeleteSubcomanda(idSubcomanda);
            return NoContent();
        }

        // DELETE: api/Subcomanda/delete-subcomanda?idComanda={idComanda}&idProdus={idProdus}
        [HttpDelete("delete-subcomanda")]
        public async Task<IActionResult> DeleteSubcomandaByComandaAndProdus(int idComanda, int idProdus)
        {
            try
            {
                await _subcomandaRepository.DeleteSubcomandaByComandaAndProdus(idComanda, idProdus);
                return NoContent();
            }
            catch
            {
                return NotFound(new { message = "Suborder not found for the given order and product." });
            }
        }
    }
}
