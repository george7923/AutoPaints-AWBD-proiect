using LicentaInAngular.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public interface ISubcomandaRepository
    {
        Task<IEnumerable<Produs>> GetProdusByComandaId(int idComanda);

        Task<IEnumerable<Produs>> GetAllProductsFromSubcomenzi();

        Task<IEnumerable<Produs>> GetProdusByUserId(int idUser);

        Task<IEnumerable<Subcomanda>> GetAllSubcomenzi();

        Task<Subcomanda> GetSubcomandaById(int idSubcomanda);

        Task AddSubcomanda(Subcomanda subcomanda);

        Task DeleteSubcomanda(int idSubcomanda);

        Task DeleteSubcomandaByComandaAndProdus(int idComanda, int idProdus);

        // Metode Response mutate din SubcomandaController
        Task<IActionResult> GetProdusByComandaIdResponse(int idComanda);

        Task<IActionResult> GetAllSubcomenziResponse();

        Task<IActionResult> GetSubcomandaByIdResponse(int idSubcomanda);

        Task<IActionResult> GetAllProductsFromSubcomenziResponse();

        Task<IActionResult> GetProdusByUserIdResponse(int idUser);

        Task<IActionResult> CreateSubcomandaResponse(Subcomanda subcomanda);

        Task<IActionResult> DeleteSubcomandaResponse(int idSubcomanda);

        Task<IActionResult> DeleteSubcomandaByComandaAndProdusResponse(int idComanda, int idProdus);
    }
}
