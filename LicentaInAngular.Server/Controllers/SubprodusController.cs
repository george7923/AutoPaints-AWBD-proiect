using Microsoft.AspNetCore.Mvc;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Models;
using System.Threading.Tasks;
using LicentaInAngular.Server.Data;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.DataLayer.DTO;


namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubprodusController : ControllerBase
    {
        private readonly ISubprodusRepository _subprodusRepository;
        private ApplicationDbContext _context;

        public SubprodusController(ISubprodusRepository subprodusRepository, ApplicationDbContext context)
        {
            _subprodusRepository = subprodusRepository;
            _context = context;
        }



        // GET: api/Subprodus
        [HttpGet]
        public async Task<IActionResult> GetAllSubproduse()
        {
            var subproduse = await _subprodusRepository.GetAllSubproduse();
            return Ok(subproduse);
        }

        // GET: api/Subprodus/{idSubprodus}
        [HttpGet("{idSubprodus}")]
        public async Task<IActionResult> GetSubprodusById(int idSubprodus)
        {
            var subprodus = await _subprodusRepository.GetSubprodusById(idSubprodus);
            if (subprodus == null)
            {
                return NotFound();
            }

            return Ok(subprodus);
        }

        // POST: api/Subprodus
        [HttpPost]
        public async Task<IActionResult> CreateSubprodus([FromBody] Subprodus subprodus)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Count > 0)
                    .SelectMany(ms => ms.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Model invalid", details = errors });
            }


            subprodus.Produs = null;
            subprodus.Cos = null;

            try
            {
                await _subprodusRepository.CreateSubprodus(subprodus);
                return CreatedAtAction(nameof(GetSubprodusById),
                    new { idSubprodus = subprodus.IdSubprodus },
                    subprodus);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // DELETE: api/Subprodus/{idSubprodus}
        [HttpDelete("{idSubprodus}")]
        public async Task<IActionResult> DeleteSubprodus(int idSubprodus)
        {
            await _subprodusRepository.DeleteSubprodus(idSubprodus);
            return NoContent();
        }

        // DELETE api/Subprodus/delete-subprodus?idCos={idCos}&idProdus={idProdus}
        [HttpDelete("delete-subprodus")]
        public async Task<IActionResult> DeleteSubprodus(int idCos, int idProdus)
        {
            try
            {
                await _subprodusRepository.DeleteSubprodus(idCos, idProdus);
                return NoContent(); // Return 204 if successful
            }
            catch (Exception ex)
            {
                return NotFound(new { message = "Subprodus not found for the given idCos and IdProdus." });
            }
        }

        // DELETE api/Subprodus/delete-by-produs/{idProdus}
        [HttpDelete("delete-by-produs/{idProdus}")]
        public async Task<IActionResult> DeleteSubproduseByProdusId(int idProdus)
        {
            try
            {
                await _subprodusRepository.DeleteSubproduseByProdusId(idProdus);
                return NoContent(); // Return 204 if successful
            }
            catch (Exception ex)
            {
                return NotFound(new { message = "Error deleting Subproduse for the given IdProdus." });
            }
        }

        // DELETE api/Subprodus/delete-by-cos/{idCos}
        [HttpDelete("delete-by-cos/{idCos}")]
        public async Task<IActionResult> DeleteSubproduseByCosId(int idCos)
        {
            try
            {
                await _subprodusRepository.DeleteSubproduseByCosId(idCos);
                return NoContent(); // Return 204 if successful
            }
            catch (Exception ex)
            {
                return NotFound(new { message = "Error deleting Subproduse for the given idCos." });
            }
        }

        // GET: api/Subprodus/by-produs/{idProdus}
        [HttpGet("by-produs/{idProdus}")]
        public async Task<IActionResult> GetSubprodusByProdusId(int idProdus)
        {
            var subproduse = await _subprodusRepository.GetSubprodusByProdusId(idProdus);
            return Ok(subproduse);
        }
        [HttpGet("count-by-produs/{idProdus}")]
        public async Task<IActionResult> CountSubproduseByProdusId(int idProdus)
        {
            int count = await _subprodusRepository.CountSubproduseByProdusId(idProdus);
            return Ok(new { Count = count });
        }

        // GET: api/Subprodus/is-valabil/{idSubprodus}
        [HttpGet("is-valabil/{idSubprodus}")]
        public async Task<IActionResult> IsSubprodusValabil(int idSubprodus)
        {
            bool isValabil = await _subprodusRepository.IsSubprodusValabil(idSubprodus);
            return Ok(new { IsValabil = isValabil });
        }
        [HttpGet("count-available-subproduse/{idProdus}")]
        public async Task<IActionResult> CountAvailableSubproduseByProdusId(int idProdus)
        {
            int count = await _subprodusRepository.CountAvailableSubproduseByProdusId(idProdus);
            return Ok(new { Count = count });
        }
        [HttpGet("countAvailable/{produsId}")]
        public async Task<IActionResult> CountAvailableSubproduse(int produsId)
        {
            var count = await _context.SubProduse
                .Where(s => s.IdProdus == produsId && s.idCos == null && s.Valabil == true)
                .CountAsync();

            return Ok(count);
        }










        [HttpGet("join-la-toate-produsele/{idCos}")]
        public async Task<IActionResult> GetProdusByCos_JoinLaToateProdusele(int idCos)
        {
            var produse = await _subprodusRepository.GetProdusByCos_JoinLaToateProdusele(idCos);
            if (produse == null || !produse.Any())
            {
                return NotFound(new { message = "No products found for this Cos." });
            }
            return Ok(produse);
        }





    }
}
