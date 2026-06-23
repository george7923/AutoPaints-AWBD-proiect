using Microsoft.AspNetCore.Mvc;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Models;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubprodusController : ControllerBase
    {
        private readonly SubprodusService _subprodusService;

        public SubprodusController(SubprodusService subprodusService)
        {
            _subprodusService = subprodusService;
        }

        // GET: api/Subprodus
        [HttpGet]
        public async Task<IActionResult> GetAllSubproduse()
        {
            return await _subprodusService.GetAllSubproduseResponse();
        }

        // GET: api/Subprodus/{idSubprodus}
        [HttpGet("{idSubprodus}")]
        public async Task<IActionResult> GetSubprodusById(int idSubprodus)
        {
            return await _subprodusService.GetSubprodusByIdResponse(idSubprodus);
        }

        // POST: api/Subprodus
        [HttpPost]
        public async Task<IActionResult> CreateSubprodus([FromBody] Subprodus subprodus)
        {
            return await _subprodusService.CreateSubprodusResponse(subprodus, ModelState);
        }

        // DELETE: api/Subprodus/{idSubprodus}
        [HttpDelete("{idSubprodus}")]
        public async Task<IActionResult> DeleteSubprodus(int idSubprodus)
        {
            return await _subprodusService.DeleteSubprodusResponse(idSubprodus);
        }

        // DELETE api/Subprodus/delete-subprodus?idCos={idCos}&idProdus={idProdus}
        [HttpDelete("delete-subprodus")]
        public async Task<IActionResult> DeleteSubprodus(int idCos, int idProdus)
        {
            return await _subprodusService.DeleteSubprodusByCosAndProdusResponse(idCos, idProdus);
        }

        // DELETE api/Subprodus/delete-by-produs/{idProdus}
        [HttpDelete("delete-by-produs/{idProdus}")]
        public async Task<IActionResult> DeleteSubproduseByProdusId(int idProdus)
        {
            return await _subprodusService.DeleteSubproduseByProdusIdResponse(idProdus);
        }

        // DELETE api/Subprodus/delete-by-cos/{idCos}
        [HttpDelete("delete-by-cos/{idCos}")]
        public async Task<IActionResult> DeleteSubproduseByCosId(int idCos)
        {
            return await _subprodusService.DeleteSubproduseByCosIdResponse(idCos);
        }

        // GET: api/Subprodus/by-produs/{idProdus}
        [HttpGet("by-produs/{idProdus}")]
        public async Task<IActionResult> GetSubprodusByProdusId(int idProdus)
        {
            return await _subprodusService.GetSubprodusByProdusIdResponse(idProdus);
        }

        [HttpGet("count-by-produs/{idProdus}")]
        public async Task<IActionResult> CountSubproduseByProdusId(int idProdus)
        {
            return await _subprodusService.CountSubproduseByProdusIdResponse(idProdus);
        }

        // GET: api/Subprodus/is-valabil/{idSubprodus}
        [HttpGet("is-valabil/{idSubprodus}")]
        public async Task<IActionResult> IsSubprodusValabil(int idSubprodus)
        {
            return await _subprodusService.IsSubprodusValabilResponse(idSubprodus);
        }

        [HttpGet("count-available-subproduse/{idProdus}")]
        public async Task<IActionResult> CountAvailableSubproduseByProdusId(int idProdus)
        {
            return await _subprodusService.CountAvailableSubproduseByProdusIdResponse(idProdus);
        }

        [HttpGet("countAvailable/{produsId}")]
        public async Task<IActionResult> CountAvailableSubproduse(int produsId)
        {
            return await _subprodusService.CountAvailableSubproduseResponse(produsId);
        }

        [HttpGet("join-la-toate-produsele/{idCos}")]
        public async Task<IActionResult> GetProdusByCos_JoinLaToateProdusele(int idCos)
        {
            return await _subprodusService.GetProdusByCos_JoinLaToateProduseleResponse(idCos);
        }
    }
}