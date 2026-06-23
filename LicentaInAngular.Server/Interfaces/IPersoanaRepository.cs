using LicentaInAngular.Server.Controllers;
using LicentaInAngular.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public interface IPersoanaRepository
    {
        Task<IEnumerable<Persoana>> GetAll();

        Task<Persoana?> GetById(int id);

        Task<Persoana?> GetByEmail(string email);

        Task CreatePersoana(Persoana persoana);

        Task UpdatePersoanaById(int id, Persoana updatedPersoana);

        Task DeletePersoanaById(int id);

        // Metode Response mutate din PersoanaController
        Task<IActionResult> GetByIdResponse(int id);

        Task<IActionResult> GetByEmailResponse(string email);

        Task<IActionResult> CreateResponse(Persoana persoana);

        Task<IActionResult> UpdatePersoanaByIdResponse(int id, Persoana updatedPersoana);

        Task<IActionResult> DeletePersoanaByIdResponse(int id);

        IActionResult SendEmailResponse(PersoanaController.EmailRequest request);
    }
}
