using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdresaController : ControllerBase
    {
        private readonly AdresaService _adresaService;

        public AdresaController(AdresaService adresaService)
        {
            _adresaService = adresaService;
        }

        // GET: api/adresa/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return await _adresaService.GetByIdResponse(id);
        }

        // CREATE ADDRESS
        [HttpPost("createForUser/{userId}")]
        public async Task<IActionResult> CreateAdresaForUser(int userId, [FromBody] AdresaNestedDTO adresaDto)
        {
            return await _adresaService.CreateAdresaForUserResponse(userId, adresaDto, ModelState.IsValid);
        }

        // UPDATE ADDRESS
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAdresaById(int id, [FromBody] AdresaNestedDTO updatedAdresaDTO)
        {
            return await _adresaService.UpdateAdresaByIdResponse(id, updatedAdresaDTO, ModelState.IsValid);
        }

        // DELETE ADDRESS
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAdresaById(int id)
        {
            return await _adresaService.DeleteAdresaByIdResponse(id);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignAddressToUser([FromBody] UserAddressDTO userAddressDto)
        {
            return await _adresaService.AssignAddressToUserResponse(userAddressDto, ModelState.IsValid);
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveAddressFromUser([FromBody] UserAddressDTO userAddressDto)
        {
            return await _adresaService.RemoveAddressFromUserResponse(userAddressDto, ModelState.IsValid);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAddressesByUser(int userId)
        {
            return await _adresaService.GetAddressesByUserResponse(userId);
        }

        [HttpGet("{adresaId}/users")]
        public async Task<IActionResult> GetUsersByAddress(int adresaId)
        {
            return await _adresaService.GetUsersByAddressResponse(adresaId);
        }

        [HttpGet("user/{userId}/simplified")]
        public async Task<IActionResult> GetSimplifiedAddressesByUser(int userId)
        {
            return await _adresaService.GetSimplifiedAddressesByUserResponse(userId);
        }
    }
}