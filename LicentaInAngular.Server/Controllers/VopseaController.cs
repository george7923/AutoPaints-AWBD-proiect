using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VopseaController : ControllerBase
    {
        private readonly VopseaService _vopseaService;

        public VopseaController(VopseaService vopseaService)
        {
            _vopseaService = vopseaService;
        }

        [HttpPost("creare-si-adauga-in-cos")]
        public async Task<IActionResult> CreareVopseaSiAdaugaInCos([FromBody] CreateVopseaSiAdaugaInCosDto dto)
        {
            return await _vopseaService.CreareVopseaSiAdaugaInCosResponse(dto);
        }
    }
}