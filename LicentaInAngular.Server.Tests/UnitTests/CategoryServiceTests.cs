using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.UnitTests
{
    [TestFixture]
    public class CategoryServiceTests
    {
        private ApplicationDbContext _context = null!;
        private CategoryService _service = null!;

        [SetUp]
        public void Before()
        {
            _context = TestDbContextFactory.CreateContext();
            _service = new CategoryService(_context);
        }

        [TearDown]
        public void After()
        {
            _context.Dispose();
        }

        private static object? GetAnonymousProperty(object source, string propertyName)
        {
            return source.GetType().GetProperty(propertyName)?.GetValue(source);
        }

        private static Categorii CreateCategory(
            int id = 1,
            string name = "Spray",
            string description = "Spray auto")
        {
            return new Categorii
            {
                IdCategorie = id,
                DenumireCategorie = name,
                DescriereCategorie = description
            };
        }

        private static CategoryDTO CreateCategoryDto(
            string name = "Vopsea",
            string description = "Vopsea auto")
        {
            return new CategoryDTO
            {
                DenumireCategorie = name,
                DescriereCategorie = description
            };
        }

        [Test]
        public async Task AddCategory_ShouldPersistCategory()
        {
            var category = CreateCategory(name: "Spray", description: "Spray auto");

            await _service.AddCategory(category);

            ClassicAssert.AreEqual(1, _context.Categorii.Count(c => c.DenumireCategorie == "Spray"));
        }

        [Test]
        public async Task GetAllCategories_ShouldReturnCategories()
        {
            _context.Categorii.AddRange(
                CreateCategory(1, "A", "DA"),
                CreateCategory(2, "B", "DB")
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetAllCategories();

            ClassicAssert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetAllCategories_WhenEmpty_ShouldReturnEmptyList()
        {
            var result = await _service.GetAllCategories();

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetCategoryById_WhenExists_ShouldReturnCategory()
        {
            _context.Categorii.Add(CreateCategory(7, "Vopsea", "Test"));
            await _context.SaveChangesAsync();

            var result = await _service.GetCategoryById(7);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("Vopsea", result!.DenumireCategorie);
            ClassicAssert.AreEqual("Test", result.DescriereCategorie);
        }

        [Test]
        public async Task GetCategoryById_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetCategoryById(999);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task GetCategoryByName_WhenExists_ShouldReturnCategory()
        {
            _context.Categorii.Add(CreateCategory(1, "Accesorii", "Test"));
            await _context.SaveChangesAsync();

            var result = await _service.GetCategoryByName("Accesorii");

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("Accesorii", result!.DenumireCategorie);
        }

        [Test]
        public async Task GetCategoryByName_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetCategoryByName("Inexistent");

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task UpdateCategory_ShouldModifyCategory()
        {
            var category = CreateCategory(9, "Old", "Old");

            _context.Categorii.Add(category);
            await _context.SaveChangesAsync();

            category.DenumireCategorie = "New";
            category.DescriereCategorie = "New desc";

            await _service.UpdateCategory(category);

            var updated = await _context.Categorii.FindAsync(9);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("New", updated!.DenumireCategorie);
            ClassicAssert.AreEqual("New desc", updated.DescriereCategorie);
        }

        [Test]
        public async Task DeleteCategory_WhenExists_ShouldRemoveCategory()
        {
            _context.Categorii.Add(CreateCategory(11, "Delete", "D"));
            await _context.SaveChangesAsync();

            await _service.DeleteCategory(11);

            var deleted = await _context.Categorii.FindAsync(11);

            ClassicAssert.IsNull(deleted);
        }

        [Test]
        public async Task DeleteCategory_WhenMissing_ShouldNotThrow()
        {
            await _service.DeleteCategory(999);

            ClassicAssert.AreEqual(0, _context.Categorii.Count());
        }

        [Test]
        public async Task GetAllCategoriesResponse_ShouldReturnOkWithCategories()
        {
            _context.Categorii.AddRange(
                CreateCategory(1, "A", "DA"),
                CreateCategory(2, "B", "DB")
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetAllCategoriesResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var categories = ok.Value as IEnumerable<Categorii>;

            ClassicAssert.IsNotNull(categories);
            ClassicAssert.AreEqual(2, categories!.Count());
        }

        [Test]
        public async Task GetCategoryByIdResponse_WhenCategoryExists_ShouldReturnOk()
        {
            _context.Categorii.Add(CreateCategory(1, "Spray", "Spray auto"));
            await _context.SaveChangesAsync();

            var result = await _service.GetCategoryByIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.IsInstanceOf<Categorii>(ok.Value);

            var category = ok.Value as Categorii;

            ClassicAssert.IsNotNull(category);
            ClassicAssert.AreEqual(1, category!.IdCategorie);
            ClassicAssert.AreEqual("Spray", category.DenumireCategorie);
        }

        [Test]
        public async Task GetCategoryByIdResponse_WhenCategoryMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetCategoryByIdResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Category not found.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task AddCategoryResponse_WhenCategoryIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.AddCategoryResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid category data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddCategoryResponse_WhenNameIsEmpty_ShouldReturnBadRequest()
        {
            var category = CreateCategory(1, "", "Descriere");

            var result = await _service.AddCategoryResponse(category);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid category data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddCategoryResponse_WhenNameIsWhiteSpace_ShouldReturnBadRequest()
        {
            var category = CreateCategory(1, "   ", "Descriere");

            var result = await _service.AddCategoryResponse(category);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid category data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddCategoryResponse_WhenValid_ShouldReturnCreatedAtAction()
        {
            var category = CreateCategory(1, "Spray", "Spray auto");

            var result = await _service.AddCategoryResponse(category);

            var created = result as CreatedAtActionResult;

            ClassicAssert.IsNotNull(created);
            ClassicAssert.AreEqual(201, created!.StatusCode);
            ClassicAssert.AreEqual("GetCategoryById", created.ActionName);
            ClassicAssert.AreEqual("Categories", created.ControllerName);
            ClassicAssert.IsInstanceOf<Categorii>(created.Value);

            var value = created.Value as Categorii;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.AreEqual("Spray", value!.DenumireCategorie);
            ClassicAssert.AreEqual(1, _context.Categorii.Count());
        }

        [Test]
        public async Task UpdateCategoryResponse_WhenIdMismatch_ShouldReturnBadRequest()
        {
            var category = CreateCategory(1, "Spray", "Spray auto");

            var result = await _service.UpdateCategoryResponse(2, category);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Category ID mismatch.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task UpdateCategoryResponse_WhenCategoryMissing_ShouldReturnNotFound()
        {
            var category = CreateCategory(999, "Spray", "Spray auto");

            var result = await _service.UpdateCategoryResponse(999, category);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Category not found.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task UpdateCategoryResponse_WhenValid_ShouldReturnNoContentAndUpdateCategory()
        {
            var category = CreateCategory(1, "Old", "Old desc");

            _context.Categorii.Add(category);
            await _context.SaveChangesAsync();

            category.DenumireCategorie = "New";
            category.DescriereCategorie = "New desc";

            var result = await _service.UpdateCategoryResponse(1, category);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);

            var updated = await _context.Categorii.FindAsync(1);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("New", updated!.DenumireCategorie);
            ClassicAssert.AreEqual("New desc", updated.DescriereCategorie);
        }

        [Test]
        public async Task DeleteCategoryResponse_WhenCategoryMissing_ShouldReturnNotFound()
        {
            var result = await _service.DeleteCategoryResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Category not found.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task DeleteCategoryResponse_WhenCategoryExists_ShouldReturnNoContent()
        {
            _context.Categorii.Add(CreateCategory(1, "Spray", "Spray auto"));
            await _context.SaveChangesAsync();

            var result = await _service.DeleteCategoryResponse(1);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
            ClassicAssert.AreEqual(0, _context.Categorii.Count());
        }

        [Test]
        public async Task AddCategoryWithoutIdResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.AddCategoryWithoutIdResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid category data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddCategoryWithoutIdResponse_WhenNameIsEmpty_ShouldReturnBadRequest()
        {
            var dto = CreateCategoryDto("", "Descriere");

            var result = await _service.AddCategoryWithoutIdResponse(dto);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid category data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddCategoryWithoutIdResponse_WhenNameIsWhiteSpace_ShouldReturnBadRequest()
        {
            var dto = CreateCategoryDto("   ", "Descriere");

            var result = await _service.AddCategoryWithoutIdResponse(dto);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid category data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddCategoryWithoutIdResponse_WhenValid_ShouldCreateCategoryAndReturnCreatedAtAction()
        {
            var dto = CreateCategoryDto("Vopsea", "Vopsea auto");

            var result = await _service.AddCategoryWithoutIdResponse(dto);

            var created = result as CreatedAtActionResult;

            ClassicAssert.IsNotNull(created);
            ClassicAssert.AreEqual(201, created!.StatusCode);
            ClassicAssert.AreEqual("GetCategoryById", created.ActionName);
            ClassicAssert.AreEqual("Categories", created.ControllerName);
            ClassicAssert.IsInstanceOf<Categorii>(created.Value);

            var value = created.Value as Categorii;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.AreEqual("Vopsea", value!.DenumireCategorie);
            ClassicAssert.AreEqual("Vopsea auto", value.DescriereCategorie);
            ClassicAssert.AreEqual(1, _context.Categorii.Count());
        }

        [Test]
        public async Task UpdateCategoryWithoutIdResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.UpdateCategoryWithoutIdResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid category data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task UpdateCategoryWithoutIdResponse_WhenNameIsEmpty_ShouldReturnBadRequest()
        {
            var dto = CreateCategoryDto("", "Descriere noua");

            var result = await _service.UpdateCategoryWithoutIdResponse(dto);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid category data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task UpdateCategoryWithoutIdResponse_WhenNameIsWhiteSpace_ShouldReturnBadRequest()
        {
            var dto = CreateCategoryDto("   ", "Descriere noua");

            var result = await _service.UpdateCategoryWithoutIdResponse(dto);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid category data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task UpdateCategoryWithoutIdResponse_WhenCategoryMissing_ShouldReturnNotFound()
        {
            var dto = CreateCategoryDto("Inexistent", "Descriere noua");

            var result = await _service.UpdateCategoryWithoutIdResponse(dto);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Category not found for update.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task UpdateCategoryWithoutIdResponse_WhenValid_ShouldReturnNoContentAndUpdateDescription()
        {
            _context.Categorii.Add(CreateCategory(1, "Spray", "Descriere veche"));
            await _context.SaveChangesAsync();

            var dto = CreateCategoryDto("Spray", "Descriere noua");

            var result = await _service.UpdateCategoryWithoutIdResponse(dto);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);

            var updated = await _context.Categorii.FindAsync(1);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("Spray", updated!.DenumireCategorie);
            ClassicAssert.AreEqual("Descriere noua", updated.DescriereCategorie);
        }
    }
}