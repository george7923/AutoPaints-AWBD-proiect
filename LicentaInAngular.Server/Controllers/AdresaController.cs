using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdresaController : ControllerBase
    {
        private readonly IAdresaRepository _adresaRepository;

        public AdresaController(IAdresaRepository adresaRepository)
        {
            _adresaRepository = adresaRepository;
        }

        // GET: api/adresa/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var adresa = await _adresaRepository.GetById(id);
            if (adresa == null)
            {
                return NotFound(new { error = "Address not found" });
            }
            return Ok(adresa);
        }

        // 🔹 CREATE ADDRESS
        [HttpPost("createForUser/{userId}")]
        public async Task<ActionResult> CreateAdresaForUser(int userId, [FromBody] AdresaNestedDTO adresaDto)
        {
            if (adresaDto == null || !ModelState.IsValid)
            {
                return BadRequest(new { error = "Invalid request data." });
            }

            // Metoda unificată: creează entitățile + adresa + asociază user
            var newAdresa = await _adresaRepository.CreateAdresaSiAsigneazaUser(adresaDto, userId);

            // Returnezi 201 + adresa
            return CreatedAtAction(nameof(GetById),
                new { id = newAdresa.IdAdresa },
                newAdresa
            );
        }


        // 🔹 UPDATE ADDRESS
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateAdresaById(int id, [FromBody] AdresaNestedDTO updatedAdresaDTO)
        {
            if (updatedAdresaDTO == null || !ModelState.IsValid)
            {
                return BadRequest(new { error = "Invalid request data" });
            }

            try
            {
                await _adresaRepository.UpdateAdresaById(id, updatedAdresaDTO);
                return Ok(new { message = "Address updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An error occurred while updating the address",
                    details = ex.Message
                });
            }
        }

        // 🔹 DELETE ADDRESS
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteAdresaById(int id)
        {
            await _adresaRepository.DeleteAdresaById(id);
            return NoContent();
        }

        
        [HttpPost("assign")]
        public async Task<IActionResult> AssignAddressToUser([FromBody] UserAddressDTO userAddressDto)
        {
            if (userAddressDto == null || !ModelState.IsValid)
                return BadRequest(new { error = "Invalid request data." });

            try
            {
                await _adresaRepository.AssignAddressToUser(userAddressDto.IdUser, userAddressDto.IdAdresa);
                return Ok(new { message = "Address assigned to user successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while assigning the address.", details = ex.Message });
            }
        }


        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveAddressFromUser([FromBody] UserAddressDTO userAddressDto)
        {
            if (userAddressDto == null || !ModelState.IsValid)
                return BadRequest(new { error = "Invalid request data." });

            try
            {
                await _adresaRepository.RemoveAddressFromUser(userAddressDto.IdUser, userAddressDto.IdAdresa);
                return Ok(new { message = "Address removed from user successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while removing the address.", details = ex.Message });
            }
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAddressesByUser(int userId)
        {
            var addresses = await _adresaRepository.GetAddressesByUser(userId);
            return Ok(new { addresses = addresses });
        }



        [HttpGet("{adresaId}/users")]
        public async Task<IActionResult> GetUsersByAddress(int adresaId)
        {
            var users = await _adresaRepository.GetUsersByAddress(adresaId);
            return Ok(users);
        }
        [HttpGet("user/{userId}/simplified")]
        public async Task<IActionResult> GetSimplifiedAddressesByUser(int userId)
        {
            var addresses = await _adresaRepository.GetAddressesByUser(userId);

            var simplifiedAddresses = addresses.Select(addr =>
                $"{addr.Strazi.DenumireStrada}, {addr.Strazi.Nr}, " +
                $"Bloc: {addr.Bloc}, Scara: {addr.Scara}, Etaj: {addr.Etaj}, Apartament: {addr.Apartament}, " +
                $"{addr.Strazi.Localitati.DenumireLocalitate}, " +
                $"{addr.Strazi.Localitati.Judete.DenumireJudet}, " +
                $"{addr.Strazi.Localitati.Judete.Tari.DenumireTara}"
            ).ToList();

            return Ok(simplifiedAddresses);
        }

    }
}
