using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public interface ISubprodusRepository
    {
        Task<IEnumerable<ProdusCosDTO>> GetProdusByCosId(int idCos);

        Task<IEnumerable<Subprodus>> GetAllSubproduse();

        Task<Subprodus> GetSubprodusById(int idSubprodus);

        Task CreateSubprodus(Subprodus subprodus);

        Task DeleteSubprodus(int idSubprodus);

        Task DeleteSubprodus(int idCos, int idProdus);

        Task DeleteSubproduseByProdusId(int idProdus);

        Task DeleteSubproduseByCosId(int idCos);

        Task<IEnumerable<Subprodus>> GetSubprodusByProdusId(int idProdus);

        Task<bool> IsSubprodusValabil(int idSubprodus);

        Task<int> CountSubproduseByProdusId(int idProdus);

        Task<int> CountAvailableSubproduseByProdusId(int idProdus);

        Task<List<Subprodus>> GetAvailableSubproduse(int idProdus, int cantitate);

        Task UpdateSubproduse(List<Subprodus> subproduse);

        Task<IEnumerable<ProdusCosDTO_2>> GetProdusByCos_JoinLaToateProdusele(int idCos);

        // Metode Response mutate din SubprodusController
        Task<IActionResult> GetAllSubproduseResponse();

        Task<IActionResult> GetSubprodusByIdResponse(int idSubprodus);

        Task<IActionResult> CreateSubprodusResponse(Subprodus subprodus, ModelStateDictionary modelState);

        Task<IActionResult> DeleteSubprodusResponse(int idSubprodus);

        Task<IActionResult> DeleteSubprodusByCosAndProdusResponse(int idCos, int idProdus);

        Task<IActionResult> DeleteSubproduseByProdusIdResponse(int idProdus);

        Task<IActionResult> DeleteSubproduseByCosIdResponse(int idCos);

        Task<IActionResult> GetSubprodusByProdusIdResponse(int idProdus);

        Task<IActionResult> CountSubproduseByProdusIdResponse(int idProdus);

        Task<IActionResult> IsSubprodusValabilResponse(int idSubprodus);

        Task<IActionResult> CountAvailableSubproduseByProdusIdResponse(int idProdus);

        Task<IActionResult> CountAvailableSubproduseResponse(int produsId);

        Task<IActionResult> GetProdusByCos_JoinLaToateProduseleResponse(int idCos);
    }
}
