using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using Microsoft.AspNetCore.Mvc;
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

        // Metode Response mutate din AdresaController
        Task<IActionResult> GetByIdResponse(int id);

        Task<IActionResult> CreateAdresaForUserResponse(int userId, AdresaNestedDTO adresaDto, bool isModelValid);

        Task<IActionResult> UpdateAdresaByIdResponse(int id, AdresaNestedDTO updatedAdresaDTO, bool isModelValid);

        Task<IActionResult> DeleteAdresaByIdResponse(int id);

        Task<IActionResult> AssignAddressToUserResponse(UserAddressDTO userAddressDto, bool isModelValid);

        Task<IActionResult> RemoveAddressFromUserResponse(UserAddressDTO userAddressDto, bool isModelValid);

        Task<IActionResult> GetAddressesByUserResponse(int userId);

        Task<IActionResult> GetUsersByAddressResponse(int adresaId);

        Task<IActionResult> GetSimplifiedAddressesByUserResponse(int userId);
    }
}
