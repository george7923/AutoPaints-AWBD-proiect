using LicentaInAngular.Server.Models;
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
    }
}
