using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public interface IAdresaRepository
    {
        Task<Adresa?> GetById(int id);
        Task<Adresa> CreateAdresaSiAsigneazaUser(AdresaNestedDTO adresaDto, int userId);
        Task UpdateAdresaById(int id, AdresaNestedDTO updatedAdresaDTO);
        Task DeleteAdresaById(int id);
        Task AssignAddressToUser(int userId, int adresaId);
        Task RemoveAddressFromUser(int userId, int adresaId);
        Task<IEnumerable<Adresa>> GetAddressesByUser(int userId);
        Task<IEnumerable<User>> GetUsersByAddress(int adresaId);
    }

}
