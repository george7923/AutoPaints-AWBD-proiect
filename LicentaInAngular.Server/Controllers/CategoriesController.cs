using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.Interfaces;
using LicentaInAngular.Server.DataLayer.DTO;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categorii>>> GetAllCategories()
        {
            var categories = await _categoryRepository.GetAllCategories();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Categorii>> GetCategoryById(int id)
        {
            var category = await _categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }
            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<Categorii>> AddCategory([FromBody] Categorii category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.DenumireCategorie))
            {
                return BadRequest(new { message = "Invalid category data." });
            }
            await _categoryRepository.AddCategory(category);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.IdCategorie }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Categorii category)
        {
            if (id != category.IdCategorie)
            {
                return BadRequest(new { message = "Category ID mismatch." });
            }
            var existingCategory = await _categoryRepository.GetCategoryById(id);
            if (existingCategory == null)
            {
                return NotFound(new { message = "Category not found." });
            }
            await _categoryRepository.UpdateCategory(category);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }
            await _categoryRepository.DeleteCategory(id);
            return NoContent();
        }

        [HttpPost("without_primarykey")]
        public async Task<ActionResult<Categorii>> AddCategoryWithoutId([FromBody] CategoryDTO categoryDto)
        {
            if (categoryDto == null || string.IsNullOrWhiteSpace(categoryDto.DenumireCategorie))
            {
                return BadRequest(new { message = "Invalid category data." });
            }
            var newCategory = new Categorii
            {
                DenumireCategorie = categoryDto.DenumireCategorie,
                DescriereCategorie = categoryDto.DescriereCategorie
            };
            await _categoryRepository.AddCategory(newCategory);
            return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.IdCategorie }, newCategory);
        }

        [HttpPut("without_primarykey")]
        public async Task<IActionResult> UpdateCategoryWithoutId([FromBody] CategoryDTO categoryDto)
        {
            if (categoryDto == null || string.IsNullOrWhiteSpace(categoryDto.DenumireCategorie))
            {
                return BadRequest(new { message = "Invalid category data." });
            }
            var existingCategory = await _categoryRepository.GetCategoryByName(categoryDto.DenumireCategorie);
            if (existingCategory == null)
            {
                return NotFound(new { message = "Category not found for update." });
            }
            existingCategory.DescriereCategorie = categoryDto.DescriereCategorie;
            await _categoryRepository.UpdateCategory(existingCategory);
            return NoContent();
        }
    }
}
