using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;

namespace LicentaInAngular.Server.Repositories
{
    public class CosRepository : ICosRepository
    {
        private readonly ApplicationDbContext _context;

        public CosRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Get the cart by UserId
        public async Task<Cos?> GetCartByUserId(int userId)
        {
            return await _context.Cosuri
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.IdUser == userId);
        }

        // 2. Get products + quantity in the cart
        public async Task<List<CartProductDTO>> GetProductsByUserId(int userId)
        {
            // Find the user's cart
            var cart = await GetCartByUserId(userId);
            if (cart == null)
                return new List<CartProductDTO>();

            // Group subproducts in the cart by product
            var subGroups = await _context.SubProduse
                .Where(sp => sp.idCos == cart.idCos)
                .GroupBy(sp => sp.IdProdus)
                .Select(g => new CartProductDTO
                {
                    IdProdus = g.Key,
                    Quantity = g.Count()
                })
                .ToListAsync();

            return subGroups;
        }

        // 3. Clear all subproducts from the user's cart
        public async Task ClearCart(int userId)
        {
            var cart = await GetCartByUserId(userId);
            if (cart == null) return;

            var subproduse = _context.SubProduse
                .Where(sp => sp.idCos == cart.idCos);

            foreach (var sp in subproduse)
            {
                sp.idCos = null; // Unassign from cart
            }
            await _context.SaveChangesAsync();
        }


        public async Task<Cos?> GetById(int id)
        {
            return await _context.Cosuri
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.idCos == id);
        }

        public async Task<IEnumerable<Cos>> GetAllCarts()
        {
            return await _context.Cosuri
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task CreateCart(Cos cos)
        {
            await _context.Cosuri.AddAsync(cos);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCart(Cos cos)
        {
            _context.Entry(cos).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCart(int id)
        {
            var cos = await _context.Cosuri.FindAsync(id);
            if (cos != null)
            {
                _context.Cosuri.Remove(cos);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteOldCarts(DateTime thresholdDate)
        {
            var oldCarts = await _context.Cosuri
                .Where(c => c.DataCreare <= thresholdDate)
                .ToListAsync();

            if (oldCarts.Any())
            {
                _context.Cosuri.RemoveRange(oldCarts);
                await _context.SaveChangesAsync();
            }
        }
    }
}
