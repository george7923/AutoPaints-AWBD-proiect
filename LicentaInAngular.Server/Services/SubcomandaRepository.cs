using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public class SubcomandaRepository : ISubcomandaRepository
    {
        private readonly ApplicationDbContext _context;

        public SubcomandaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieves all products for a given order by joining Subcomenzi with Products
        public async Task<IEnumerable<Produs>> GetProdusByComandaId(int idComanda)
        {
            var products = await (from sc in _context.Subcomenzi
                                  join p in _context.Products on sc.IdProdus equals p.IdProdus
                                  where sc.IdComanda == idComanda
                                  select p).ToListAsync();
            return products;
        }

        // Retrieves a distinct list of products that appear in any suborder
        public async Task<IEnumerable<Produs>> GetAllProductsFromSubcomenzi()
        {
            var products = await (from sc in _context.Subcomenzi
                                  join p in _context.Products on sc.IdProdus equals p.IdProdus
                                  select p).Distinct().ToListAsync();
            return products;
        }

        // Retrieves products ordered by a specific user (by joining Subcomenzi, Comenzi, and Products)
        public async Task<IEnumerable<Produs>> GetProdusByUserId(int idUser)
        {
            var products = await (from sc in _context.Subcomenzi
                                  join p in _context.Products on sc.IdProdus equals p.IdProdus
                                  join c in _context.Comenzi on sc.IdComanda equals c.IdComanda
                                  where c.IdUser == idUser
                                  select p).ToListAsync();
            return products;
        }

        // Retrieves all suborder records, including their associated Product and Comanda
        public async Task<IEnumerable<Subcomanda>> GetAllSubcomenzi()
        {
            return await _context.Subcomenzi
                .Include(sc => sc.Produs)
                .Include(sc => sc.Comanda)
                .ToListAsync();
        }

        // Retrieves a single suborder record by its ID
        public async Task<Subcomanda> GetSubcomandaById(int idSubcomanda)
        {
            return await _context.Subcomenzi
                .FirstOrDefaultAsync(sc => sc.IdSubcomanda == idSubcomanda);
        }

        // Creates a new suborder record in the database
        public async Task AddSubcomanda(Subcomanda subcomanda)
        {
            await _context.Subcomenzi.AddAsync(subcomanda);
            await _context.SaveChangesAsync();
        }

        // Deletes a suborder record by its ID
        public async Task DeleteSubcomanda(int idSubcomanda)
        {
            var subcomanda = await _context.Subcomenzi.FindAsync(idSubcomanda);
            if (subcomanda != null)
            {
                _context.Subcomenzi.Remove(subcomanda);
                await _context.SaveChangesAsync();
            }
        }

        // Deletes a suborder record that matches the given order ID and product ID
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
    }
}
