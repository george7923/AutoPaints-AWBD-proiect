using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public class AdresaService : IAdresaRepository
    {
        private readonly ApplicationDbContext _context;

        public AdresaService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // METODE EXISTENTE DIN IAdresaRepository
        // =========================

        public async Task<Adresa?> GetById(int id)
        {
            return await _context.Adrese
                .Include(a => a.Strazi)
                    .ThenInclude(s => s.Localitati)
                    .ThenInclude(l => l.Judete)
                    .ThenInclude(j => j.Tari)
                .FirstOrDefaultAsync(a => a.IdAdresa == id);
        }

        public async Task<Adresa> CreateAdresaSiAsigneazaUser(AdresaNestedDTO adresaDto, int userId)
        {
            string taraName = adresaDto.Strazi?.Localitati?.Judete?.Tari?.DenumireTara;
            string judetName = adresaDto.Strazi?.Localitati?.Judete?.DenumireJudet;
            string localitateName = adresaDto.Strazi?.Localitati?.DenumireLocalitate;
            string stradaName = adresaDto.Strazi?.DenumireStrada;
            int nrStrada = adresaDto.Strazi?.Nr ?? 0;

            Tari taraExistent = null;
            if (!string.IsNullOrEmpty(taraName))
            {
                taraExistent = await _context.Tari
                    .FirstOrDefaultAsync(t => t.DenumireTara == taraName);

                if (taraExistent == null)
                {
                    taraExistent = new Tari { DenumireTara = taraName };
                    _context.Tari.Add(taraExistent);
                    await _context.SaveChangesAsync();
                }
            }

            Judete judetExistent = null;
            if (taraExistent != null && !string.IsNullOrEmpty(judetName))
            {
                judetExistent = await _context.Judete
                    .FirstOrDefaultAsync(j => j.DenumireJudet == judetName
                                           && j.IdTara == taraExistent.IdTara);

                if (judetExistent == null)
                {
                    judetExistent = new Judete
                    {
                        DenumireJudet = judetName,
                        IdTara = taraExistent.IdTara
                    };

                    _context.Judete.Add(judetExistent);
                    await _context.SaveChangesAsync();
                }
            }

            Localitati localitateExistenta = null;
            if (judetExistent != null && !string.IsNullOrEmpty(localitateName))
            {
                localitateExistenta = await _context.Localitati
                    .FirstOrDefaultAsync(l => l.DenumireLocalitate == localitateName
                                           && l.IdJudet == judetExistent.IdJudet);

                if (localitateExistenta == null)
                {
                    localitateExistenta = new Localitati
                    {
                        DenumireLocalitate = localitateName,
                        IdJudet = judetExistent.IdJudet
                    };

                    _context.Localitati.Add(localitateExistenta);
                    await _context.SaveChangesAsync();
                }
            }

            Strazi stradaExistenta = null;
            if (localitateExistenta != null && !string.IsNullOrEmpty(stradaName))
            {
                stradaExistenta = await _context.Strazi
                    .FirstOrDefaultAsync(s => s.DenumireStrada == stradaName
                                           && s.Nr == nrStrada
                                           && s.IdLocalitate == localitateExistenta.IdLocalitate);

                if (stradaExistenta == null)
                {
                    stradaExistenta = new Strazi
                    {
                        DenumireStrada = stradaName,
                        Nr = nrStrada,
                        IdLocalitate = localitateExistenta.IdLocalitate
                    };

                    _context.Strazi.Add(stradaExistenta);
                    await _context.SaveChangesAsync();
                }
            }

            var adresa = new Adresa
            {
                Bloc = adresaDto.Bloc,
                Scara = adresaDto.Scara,
                Etaj = adresaDto.Etaj,
                Apartament = adresaDto.Apartament,
                IdStrada = stradaExistenta.IdStrada
            };

            await _context.Adrese.AddAsync(adresa);
            await _context.SaveChangesAsync();

            if (adresa.IdAdresa <= 0)
                throw new Exception("ID-ul adresei nu a fost generat corect după salvare.");

            await AssociateAddressWithUser(userId, adresa.IdAdresa);

            return adresa;
        }

        private async Task AssociateAddressWithUser(int userId, int idAdresa)
        {
            bool userExists = await _context.Users.AnyAsync(u => u.IdUser == userId);
            bool addressExists = await _context.Adrese.AnyAsync(a => a.IdAdresa == idAdresa);

            if (!userExists || !addressExists)
                throw new Exception("User sau adresă inexistente în momentul asocierii.");

            var alreadyAssigned = await _context.Adrese_Useri
                .AnyAsync(au => au.IdUser == userId && au.IdAdresa == idAdresa);

            if (!alreadyAssigned)
            {
                var userAddress = new Adrese_Useri
                {
                    IdUser = userId,
                    IdAdresa = idAdresa
                };

                _context.Adrese_Useri.Add(userAddress);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAdresaById(int id, AdresaNestedDTO updatedAdresaDTO)
        {
            var existingAdresa = await _context.Adrese
                .Include(a => a.Strazi)
                    .ThenInclude(s => s.Localitati)
                    .ThenInclude(l => l.Judete)
                    .ThenInclude(j => j.Tari)
                .FirstOrDefaultAsync(a => a.IdAdresa == id);

            if (existingAdresa == null)
            {
                throw new KeyNotFoundException("Address not found.");
            }

            existingAdresa.Bloc = updatedAdresaDTO.Bloc ?? existingAdresa.Bloc;
            existingAdresa.Scara = updatedAdresaDTO.Scara ?? existingAdresa.Scara;
            existingAdresa.Etaj = updatedAdresaDTO.Etaj ?? existingAdresa.Etaj;
            existingAdresa.Apartament = updatedAdresaDTO.Apartament ?? existingAdresa.Apartament;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAdresaById(int id)
        {
            var adresa = await _context.Adrese.FindAsync(id);

            if (adresa != null)
            {
                _context.Adrese.Remove(adresa);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AssignAddressToUser(int userId, int adresaId)
        {
            // Check if user and address exist
            var user = await _context.Users.FindAsync(userId);
            var address = await _context.Adrese.FindAsync(adresaId);

            if (user == null || address == null)
                throw new KeyNotFoundException("User or address not found.");

            // Check if the relationship already exists
            bool alreadyAssigned = await _context.Adrese_Useri
                .AnyAsync(au => au.IdUser == userId && au.IdAdresa == adresaId);

            if (alreadyAssigned)
                throw new InvalidOperationException("Address is already assigned to this user.");

            // Add the association
            var userAddress = new Adrese_Useri
            {
                IdUser = userId,
                IdAdresa = adresaId
            };

            await _context.Adrese_Useri.AddAsync(userAddress);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAddressFromUser(int userId, int adresaId)
        {
            var userAddress = await _context.Adrese_Useri
                .FirstOrDefaultAsync(au => au.IdUser == userId && au.IdAdresa == adresaId);

            if (userAddress == null)
                throw new KeyNotFoundException("The address is not assigned to this user.");

            _context.Adrese_Useri.Remove(userAddress);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Adresa>> GetAddressesByUser(int userId)
        {
            var adresUseri = await _context.Adrese_Useri
                .Where(au => au.IdUser == userId)
                .Include(au => au.Adrese)
                    .ThenInclude(a => a.Strazi)
                        .ThenInclude(s => s.Localitati)
                            .ThenInclude(l => l.Judete)
                                .ThenInclude(j => j.Tari)
                .ToListAsync();

            return adresUseri.Select(au => au.Adrese);
        }

        public async Task<IEnumerable<User>> GetUsersByAddress(int adresaId)
        {
            return await _context.Adrese_Useri
                .Where(au => au.IdAdresa == adresaId)
                .Select(au => au.User)
                .Include(u => u.Persoana)
                .ToListAsync();
        }

        // =========================
        // LOGICA MUTATA DIN AdresaController
        // =========================

        public async Task<IActionResult> GetByIdResponse(int id)
        {
            var adresa = await GetById(id);

            if (adresa == null)
            {
                return new NotFoundObjectResult(new { error = "Address not found" });
            }

            return new OkObjectResult(adresa);
        }

        public async Task<IActionResult> CreateAdresaForUserResponse(int userId, AdresaNestedDTO adresaDto, bool isModelValid)
        {
            if (adresaDto == null || !isModelValid)
            {
                return new BadRequestObjectResult(new { error = "Invalid request data." });
            }

            // Metoda unificata: creeaza entitatile + adresa + asociaza user
            var newAdresa = await CreateAdresaSiAsigneazaUser(adresaDto, userId);

            // Returnezi 201 + adresa
            return new CreatedAtActionResult(
                "GetById",
                "Adresa",
                new { id = newAdresa.IdAdresa },
                newAdresa
            );
        }

        public async Task<IActionResult> UpdateAdresaByIdResponse(int id, AdresaNestedDTO updatedAdresaDTO, bool isModelValid)
        {
            if (updatedAdresaDTO == null || !isModelValid)
            {
                return new BadRequestObjectResult(new { error = "Invalid request data" });
            }

            try
            {
                await UpdateAdresaById(id, updatedAdresaDTO);
                return new OkObjectResult(new { message = "Address updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "An error occurred while updating the address",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> DeleteAdresaByIdResponse(int id)
        {
            await DeleteAdresaById(id);
            return new NoContentResult();
        }

        public async Task<IActionResult> AssignAddressToUserResponse(UserAddressDTO userAddressDto, bool isModelValid)
        {
            if (userAddressDto == null || !isModelValid)
            {
                return new BadRequestObjectResult(new { error = "Invalid request data." });
            }

            try
            {
                await AssignAddressToUser(userAddressDto.IdUser, userAddressDto.IdAdresa);
                return new OkObjectResult(new { message = "Address assigned to user successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "An error occurred while assigning the address.",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> RemoveAddressFromUserResponse(UserAddressDTO userAddressDto, bool isModelValid)
        {
            if (userAddressDto == null || !isModelValid)
            {
                return new BadRequestObjectResult(new { error = "Invalid request data." });
            }

            try
            {
                await RemoveAddressFromUser(userAddressDto.IdUser, userAddressDto.IdAdresa);
                return new OkObjectResult(new { message = "Address removed from user successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "An error occurred while removing the address.",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> GetAddressesByUserResponse(int userId)
        {
            var addresses = await GetAddressesByUser(userId);
            return new OkObjectResult(new { addresses = addresses });
        }

        public async Task<IActionResult> GetUsersByAddressResponse(int adresaId)
        {
            var users = await GetUsersByAddress(adresaId);
            return new OkObjectResult(users);
        }

        public async Task<IActionResult> GetSimplifiedAddressesByUserResponse(int userId)
        {
            var addresses = await GetAddressesByUser(userId);

            var simplifiedAddresses = addresses.Select(addr =>
                $"{addr.Strazi.DenumireStrada}, {addr.Strazi.Nr}, " +
                $"Bloc: {addr.Bloc}, Scara: {addr.Scara}, Etaj: {addr.Etaj}, Apartament: {addr.Apartament}, " +
                $"{addr.Strazi.Localitati.DenumireLocalitate}, " +
                $"{addr.Strazi.Localitati.Judete.DenumireJudet}, " +
                $"{addr.Strazi.Localitati.Judete.Tari.DenumireTara}"
            ).ToList();

            return new OkObjectResult(simplifiedAddresses);
        }
    }
}