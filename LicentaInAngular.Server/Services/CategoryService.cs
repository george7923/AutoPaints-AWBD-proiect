// CategoryRepository.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public class CategoryService : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // METODE EXISTENTE DIN ICategoryRepository
        // =========================

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

        // =========================
        // LOGICA MUTATA DIN CategoriesController
        // =========================

        public async Task<IActionResult> GetAllCategoriesResponse()
        {
            var categories = await GetAllCategories();
            return new OkObjectResult(categories);
        }

        public async Task<IActionResult> GetCategoryByIdResponse(int id)
        {
            var category = await GetCategoryById(id);

            if (category == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Category not found."
                });
            }

            return new OkObjectResult(category);
        }

        public async Task<IActionResult> AddCategoryResponse(Categorii category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.DenumireCategorie))
            {
                return new BadRequestObjectResult(new
                {
                    message = "Invalid category data."
                });
            }

            await AddCategory(category);

            return new CreatedAtActionResult(
                "GetCategoryById",
                "Categories",
                new { id = category.IdCategorie },
                category
            );
        }

        public async Task<IActionResult> UpdateCategoryResponse(int id, Categorii category)
        {
            if (id != category.IdCategorie)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Category ID mismatch."
                });
            }

            var existingCategory = await GetCategoryById(id);

            if (existingCategory == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Category not found."
                });
            }

            await UpdateCategory(category);

            return new NoContentResult();
        }

        public async Task<IActionResult> DeleteCategoryResponse(int id)
        {
            var category = await GetCategoryById(id);

            if (category == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Category not found."
                });
            }

            await DeleteCategory(id);

            return new NoContentResult();
        }

        public async Task<IActionResult> AddCategoryWithoutIdResponse(CategoryDTO categoryDto)
        {
            if (categoryDto == null || string.IsNullOrWhiteSpace(categoryDto.DenumireCategorie))
            {
                return new BadRequestObjectResult(new
                {
                    message = "Invalid category data."
                });
            }

            var newCategory = new Categorii
            {
                DenumireCategorie = categoryDto.DenumireCategorie,
                DescriereCategorie = categoryDto.DescriereCategorie
            };

            await AddCategory(newCategory);

            return new CreatedAtActionResult(
                "GetCategoryById",
                "Categories",
                new { id = newCategory.IdCategorie },
                newCategory
            );
        }

        public async Task<IActionResult> UpdateCategoryWithoutIdResponse(CategoryDTO categoryDto)
        {
            if (categoryDto == null || string.IsNullOrWhiteSpace(categoryDto.DenumireCategorie))
            {
                return new BadRequestObjectResult(new
                {
                    message = "Invalid category data."
                });
            }

            var existingCategory = await GetCategoryByName(categoryDto.DenumireCategorie);

            if (existingCategory == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Category not found for update."
                });
            }

            existingCategory.DescriereCategorie = categoryDto.DescriereCategorie;

            await UpdateCategory(existingCategory);

            return new NoContentResult();
        }
    }
}