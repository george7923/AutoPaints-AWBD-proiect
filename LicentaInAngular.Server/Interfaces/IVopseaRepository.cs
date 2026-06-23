using LicentaInAngular.Server.DataLayer.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public interface IVopseaRepository
    {
        Task<IActionResult> CreareVopseaSiAdaugaInCosResponse(CreateVopseaSiAdaugaInCosDto dto);
    }
}
