using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicentaInAngular.Server.DataLayer.DTO;

namespace LicentaInAngular.Server.Repositories
{
    public class ComandaRepository : IComandaRepository
    {
        private readonly ApplicationDbContext _context;

        public ComandaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get a Comanda by its ID
        public async Task<ComandaDTO?> GetById(int id)
        {
            /*
            return await _context.Comenzi
                .Include(c => c.User)
                .Include(c => c.Adresa)
                .Where(c => c.IdComanda == id)
                .(c => new ComandaDTO
                {
                    IdComanda = c.IdComanda,
                    IdUser = c.IdUser,
                    IdAdresa = c.IdAdresa,
                    ETA = c.ETA,

                    // Detalii utilizator
                    Username = c.User.Username,

                    // Detalii adresă
                    Tara = c.Adresa.Tara,
                    Judet = c.Adresa.Judet,
                    Localitate = c.Adresa.Localitate,
                    Strada = c.Adresa.Strada,
                    Bloc = c.Adresa.Bloc,
                    Scara = c.Adresa.Scara,
                    Etaj = c.Adresa.Etaj,
                    Apartament = c.Adresa.Apartament
                })
                .FirstOrDefaultAsync();
            */
            return null;
        }

        // Get all Comenzi (Orders)
        public async Task<IEnumerable<ComandaDTO>> GetAllOrders()
        {
            /*
            return await _context.Comenzi
                .Include(c => c.User)
                .Include(c => c.Adresa)
                .(c => new ComandaDTO
                {
                    IdComanda = c.IdComanda,
                    IdUser = c.IdUser,
                    IdAdresa = c.IdAdresa,
                    ETA = c.ETA,

                    // Detalii utilizator
                    Username = c.User.Username,

                    // Detalii adresă
                    Tara = c.Adresa.Tara,
                    Judet = c.Adresa.Judet,
                    Localitate = c.Adresa.Localitate,
                    Strada = c.Adresa.Strada,
                    Bloc = c.Adresa.Bloc,
                    Scara = c.Adresa.Scara,
                    Etaj = c.Adresa.Etaj,
                    Apartament = c.Adresa.Apartament
                })
                .ToListAsync();
            */
            return null;
        }

        // Create a new Comanda (Order)
        public async Task CreateComanda(Comanda comanda)
        {
            await _context.Comenzi.AddAsync(comanda);
            await _context.SaveChangesAsync();  // Save changes to the database
        }

        // Update an existing Comanda (Order)
        public async Task UpdateComanda(Comanda comanda)
        {
            _context.Entry(comanda).State = EntityState.Modified;
            await _context.SaveChangesAsync();  // Save the updates to the database
        }

        // Delete a Comanda by its ID
        public async Task DeleteComanda(int id)
        {
            var comanda = await _context.Comenzi.FindAsync(id);
            if (comanda != null)
            {
                _context.Comenzi.Remove(comanda);
                await _context.SaveChangesAsync();  // Save changes after deletion
            }
        }

        // Get all Comenzi for a User by UserId
        public async Task<IEnumerable<ComandaDTO>> GetComenziByUserId(int userId)
        {
            /*
            return await _context.Comenzi
                .Include(c => c.User)
                .Include(c => c.Adresa)
                .Where(c => c.IdUser == userId)
                .(c => new ComandaDTO
                {
                    IdComanda = c.IdComanda,
                    IdUser = c.IdUser,
                    IdAdresa = c.IdAdresa,
                    ETA = c.ETA,

                    // Detalii utilizator
                    Username = c.User.Username,

                    // Detalii adresă
                    Tara = c.Adresa.Tara,
                    Judet = c.Adresa.Judet,
                    Localitate = c.Adresa.Localitate,
                    Strada = c.Adresa.Strada,
                    Bloc = c.Adresa.Bloc,
                    Scara = c.Adresa.Scara,
                    Etaj = c.Adresa.Etaj,
                    Apartament = c.Adresa.Apartament
                })
                .ToListAsync();
            */
            return null;
        }

        public async Task<Comanda> SubmitComanda(ComandaSubmitDTO comandaDto)
        {
            
            return null;
        }
        public async Task<bool> MarcheazaComandaCaLivrata(int idComanda)
        {
            var comanda = await _context.Comenzi.FindAsync(idComanda);
            if (comanda == null)
                return false;

            comanda.IsPlaced = true;
            comanda.ETA = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<int> EmitereComanda(int userId, int idAdresa, int? idCard)
        {
            var cos = await _context.Cosuri
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.IdUser == userId);

            if (cos == null)
                throw new Exception("Coșul nu a fost găsit.");

            var subproduse = await _context.SubProduse
                .Where(sp => sp.idCos == cos.idCos && sp.Valabil)
                .Include(sp => sp.Produs)
                .ToListAsync();

            if (!subproduse.Any())
                throw new Exception("Coșul este gol.");

            double pretTotal = 0;

            foreach (var sp in subproduse)
            {
                var pret = await _context.Preturi_Produs
                    .Where(p => p.IdProdus == sp.IdProdus)
                    .OrderByDescending(p => p.DataInceput)
                    .Select(p => p.Pret)
                    .FirstOrDefaultAsync();

                pretTotal += (double)pret;
            }
            var comanda = new Comanda
            {
                IdUser = userId,
                IdAdresa = idAdresa,
                IdCard_CC = idCard,
                ETA = DateTime.UtcNow,
                IsPlaced = false,

                PretTotal = pretTotal
            };

            await _context.Comenzi.AddAsync(comanda);
            await _context.SaveChangesAsync();
            var grupate = subproduse.GroupBy(sp => sp.IdProdus);

            foreach (var grup in grupate)
            {
                var subcomanda = new Subcomanda
                {
                    IdProdus = grup.Key,
                    IdComanda = comanda.IdComanda,
                    TotalSubproduse = grup.Count()
                };

                await _context.Subcomenzi.AddAsync(subcomanda);
                
                foreach (var sp in grup)
                {
                    sp.idCos = null;

                }

            }

            await _context.SaveChangesAsync();
            return comanda.IdComanda;
        }
        public async Task<IEnumerable<ComandaCuDetaliiDTO>> GetToateComenzileAleUtilizatorului(int userId)
        {
            var comenzi = await _context.Comenzi
                .Where(c => c.IdUser == userId)
                .Include(c => c.Adresa)
                    .ThenInclude(a => a.Strazi)
                        .ThenInclude(s => s.Localitati)
                            .ThenInclude(l => l.Judete)
                                .ThenInclude(j => j.Tari)
               .Select(c => new ComandaCuDetaliiDTO
                {
                    IdComanda = c.IdComanda,
                    ETA = c.ETA,
                    IsPlaced = c.IsPlaced,
                    PretTotal = c.PretTotal,

                    // Adresa detaliata
                    Tara = c.Adresa.Strazi.Localitati.Judete.Tari.DenumireTara,
                    Judet = c.Adresa.Strazi.Localitati.Judete.DenumireJudet,
                    Localitate = c.Adresa.Strazi.Localitati.DenumireLocalitate,
                    Strada = c.Adresa.Strazi.DenumireStrada,
                    Nr = c.Adresa.Strazi.Nr,
                    Bloc = c.Adresa.Bloc,
                    Scara = c.Adresa.Scara,
                    Etaj = c.Adresa.Etaj,
                    Apartament = c.Adresa.Apartament,

                    // Produse comandate
                    Produse = _context.Subcomenzi
                        .Where(s => s.IdComanda == c.IdComanda)
                        .Select(s => new ProdusComandaDTO
                        {
                            NumeProdus = s.Produs.Nume,
                            Cantitate = s.TotalSubproduse
                        }).ToList()
                })
                .ToListAsync();

            return comenzi;
        }



    }
}
