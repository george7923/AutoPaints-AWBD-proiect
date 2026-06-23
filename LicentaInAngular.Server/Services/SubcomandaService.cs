using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public class SubcomandaService : ISubcomandaRepository
    {
        private readonly ApplicationDbContext _context;

        public SubcomandaService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // METODE EXISTENTE DIN ISubcomandaRepository
        // =========================

        public async Task<IEnumerable<Produs>> GetProdusByComandaId(int idComanda)
        {
            var products = await (from sc in _context.Subcomenzi
                                  join p in _context.Products on sc.IdProdus equals p.IdProdus
                                  where sc.IdComanda == idComanda
                                  select p).ToListAsync();

            return products;
        }

        public async Task<IEnumerable<Produs>> GetAllProductsFromSubcomenzi()
        {
            var products = await (from sc in _context.Subcomenzi
                                  join p in _context.Products on sc.IdProdus equals p.IdProdus
                                  select p).Distinct().ToListAsync();

            return products;
        }

        public async Task<IEnumerable<Produs>> GetProdusByUserId(int idUser)
        {
            var products = await (from sc in _context.Subcomenzi
                                  join p in _context.Products on sc.IdProdus equals p.IdProdus
                                  join c in _context.Comenzi on sc.IdComanda equals c.IdComanda
                                  where c.IdUser == idUser
                                  select p).ToListAsync();

            return products;
        }

        public async Task<IEnumerable<Subcomanda>> GetAllSubcomenzi()
        {
            return await _context.Subcomenzi
                .Include(sc => sc.Produs)
                .Include(sc => sc.Comanda)
                .ToListAsync();
        }

        public async Task<Subcomanda> GetSubcomandaById(int idSubcomanda)
        {
            return await _context.Subcomenzi
                .FirstOrDefaultAsync(sc => sc.IdSubcomanda == idSubcomanda);
        }

        public async Task AddSubcomanda(Subcomanda subcomanda)
        {
            await _context.Subcomenzi.AddAsync(subcomanda);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSubcomanda(int idSubcomanda)
        {
            var subcomanda = await _context.Subcomenzi.FindAsync(idSubcomanda);

            if (subcomanda != null)
            {
                _context.Subcomenzi.Remove(subcomanda);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteSubcomandaByComandaAndProdus(int idComanda, int idProdus)
        {
            var subcomanda = await _context.Subcomenzi
                .FirstOrDefaultAsync(sc => sc.IdComanda == idComanda && sc.IdProdus == idProdus);

            if (subcomanda != null)
            {
                _context.Subcomenzi.Remove(subcomanda);
                await _context.SaveChangesAsync();
            }
        }

        // =========================
        // LOGICA MUTATA DIN SubcomandaController
        // =========================

        public async Task<IActionResult> GetProdusByComandaIdResponse(int idComanda)
        {
            var produse = await GetProdusByComandaId(idComanda);

            if (produse == null || !produse.Any())
            {
                return new NotFoundObjectResult(new
                {
                    message = "No products found for this order."
                });
            }

            return new OkObjectResult(produse);
        }

        public async Task<IActionResult> GetAllSubcomenziResponse()
        {
            var subcomenzi = await GetAllSubcomenzi();
            return new OkObjectResult(subcomenzi);
        }

        public async Task<IActionResult> GetSubcomandaByIdResponse(int idSubcomanda)
        {
            var subcomanda = await GetSubcomandaById(idSubcomanda);

            if (subcomanda == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Suborder not found."
                });
            }

            return new OkObjectResult(subcomanda);
        }

        public async Task<IActionResult> GetAllProductsFromSubcomenziResponse()
        {
            var produse = await GetAllProductsFromSubcomenzi();

            if (produse == null || !produse.Any())
            {
                return new NotFoundObjectResult(new
                {
                    message = "No products found in any suborder."
                });
            }

            return new OkObjectResult(produse);
        }

        public async Task<IActionResult> GetProdusByUserIdResponse(int idUser)
        {
            var produse = await GetProdusByUserId(idUser);

            if (produse == null || !produse.Any())
            {
                return new NotFoundObjectResult(new
                {
                    message = "No products found for this user."
                });
            }

            return new OkObjectResult(produse);
        }

        public async Task<IActionResult> CreateSubcomandaResponse(Subcomanda subcomanda)
        {
            if (subcomanda == null)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Suborder data is required."
                });
            }

            await AddSubcomanda(subcomanda);

            return new CreatedAtActionResult(
                "GetSubcomandaById",
                "Subcomanda",
                new { idSubcomanda = subcomanda.IdSubcomanda },
                subcomanda
            );
        }

        public async Task<IActionResult> DeleteSubcomandaResponse(int idSubcomanda)
        {
            await DeleteSubcomanda(idSubcomanda);
            return new NoContentResult();
        }

        public async Task<IActionResult> DeleteSubcomandaByComandaAndProdusResponse(int idComanda, int idProdus)
        {
            try
            {
                await DeleteSubcomandaByComandaAndProdus(idComanda, idProdus);
                return new NoContentResult();
            }
            catch
            {
                return new NotFoundObjectResult(new
                {
                    message = "Suborder not found for the given order and product."
                });
            }
        }
    }
}