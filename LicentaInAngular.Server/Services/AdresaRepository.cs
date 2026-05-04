using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public class AdresaRepository : IAdresaRepository
    {
        private readonly ApplicationDbContext _context;

        public AdresaRepository(ApplicationDbContext context)
        {
            _context = context;
        }


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
            var userAddress = new Adrese_Useri { IdUser = userId, IdAdresa = adresaId };
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

    }

}
