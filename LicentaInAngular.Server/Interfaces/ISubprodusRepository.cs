using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
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
    }
}
