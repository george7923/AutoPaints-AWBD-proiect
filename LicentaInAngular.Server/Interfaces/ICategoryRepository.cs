using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DataLayer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Categorii>> GetAllCategories();

        Task<Categorii> GetCategoryById(int id);

        Task AddCategory(Categorii category);

        Task UpdateCategory(Categorii category);

        Task DeleteCategory(int id);

        Task<Categorii> GetCategoryByName(string denumireCategorie);

        // Metode Response mutate din CategoriesController
        Task<IActionResult> GetAllCategoriesResponse();

        Task<IActionResult> GetCategoryByIdResponse(int id);

        Task<IActionResult> AddCategoryResponse(Categorii category);

        Task<IActionResult> UpdateCategoryResponse(int id, Categorii category);

        Task<IActionResult> DeleteCategoryResponse(int id);

        Task<IActionResult> AddCategoryWithoutIdResponse(CategoryDTO categoryDto);

        Task<IActionResult> UpdateCategoryWithoutIdResponse(CategoryDTO categoryDto);
    }
}
