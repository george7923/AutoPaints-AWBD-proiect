// CategoryRepository.cs
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicentaInAngular.Server.DataLayer.Models;

namespace LicentaInAngular.Server.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Categorii>> GetAllCategories()
        {
            return await _context.Categorii.ToListAsync();
        }

        public async Task<Categorii> GetCategoryById(int id)
        {
            return await _context.Categorii.FirstOrDefaultAsync(c => c.IdCategorie == id);
        }

        public async Task AddCategory(Categorii category)
        {
            await _context.Categorii.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategory(Categorii category)
        {
            _context.Categorii.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategory(int id)
        {
            var category = await _context.Categorii.FindAsync(id);
            if (category != null)
            {
                _context.Categorii.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Categorii> GetCategoryByName(string denumireCategorie)
        {
            return await _context.Categorii
                .FirstOrDefaultAsync(c => c.DenumireCategorie == denumireCategorie);
        }
    }
}
