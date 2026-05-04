using LicentaInAngular.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public interface IPersoanaRepository
    {
        Task<IEnumerable<Persoana>> GetAll();                // Get all Persoana records
        Task<Persoana?> GetById(int id);                      // Get a specific Persoana by ID
        Task<Persoana?> GetByEmail(string email);             // Get a Persoana by email
        Task CreatePersoana(Persoana persoana);                // Create a new Persoana
        Task UpdatePersoanaById(int id, Persoana updatedPersoana); // Update Persoana by IdUser
        Task DeletePersoanaById(int id);
    }
}
