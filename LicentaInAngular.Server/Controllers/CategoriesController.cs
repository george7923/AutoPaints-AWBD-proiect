using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.DataLayer.DTO;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoriesController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            return await _categoryService.GetAllCategoriesResponse();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            return await _categoryService.GetCategoryByIdResponse(id);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] Categorii category)
        {
            return await _categoryService.AddCategoryResponse(category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Categorii category)
        {
            return await _categoryService.UpdateCategoryResponse(id, category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            return await _categoryService.DeleteCategoryResponse(id);
        }

        [HttpPost("without_primarykey")]
        public async Task<IActionResult> AddCategoryWithoutId([FromBody] CategoryDTO categoryDto)
        {
            return await _categoryService.AddCategoryWithoutIdResponse(categoryDto);
        }

        [HttpPut("without_primarykey")]
        public async Task<IActionResult> UpdateCategoryWithoutId([FromBody] CategoryDTO categoryDto)
        {
            return await _categoryService.UpdateCategoryWithoutIdResponse(categoryDto);
        }
    }
}