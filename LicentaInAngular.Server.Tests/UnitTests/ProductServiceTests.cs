using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LicentaInAngular.Server.Controllers;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.DTOs;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.UnitTests
{
    [TestFixture]
    public class ProductServiceTests
    {
        private ApplicationDbContext _context = null!;
        private ProductService _service = null!;

        [SetUp]
        public void Before()
        {
            _context = TestDbContextFactory.CreateContext();
            _service = new ProductService(_context);
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

        private static List<object> AnonymousList(object value)
        {
            return ((IEnumerable)value).Cast<object>().ToList();
        }

        private static Categorii CreateCategory(int id = 1, string name = "Vopsea", string description = "Categorie test")
        {
            return new Categorii
            {
                IdCategorie = id,
                DenumireCategorie = name,
                DescriereCategorie = description
            };
        }

        private static Produs CreateProduct(
            int id = 1,
            string name = "Produs test",
            string description = "Descriere test",
            bool esteSpray = false,
            bool valabil = true,
            int categoryId = 1,
            int? userId = 1)
        {
            return new Produs
            {
                IdProdus = id,
                Nume = name,
                Descriere = description,
                EsteSpray = esteSpray,
                Valabil = valabil,
                IdCategorie = categoryId,
                IdUser = userId
            };
        }

        private static Persoana CreatePersoana(
            int id = 1,
            string nume = "Vendor",
            string prenume = "Test",
            string tipPersoana = "Juridica",
            string rol = "Participant")
        {
            return new Persoana
            {
                IdPersoana = id,
                Nume = nume,
                Prenume = prenume,
                Email = $"vendor{id}@test.com",
                tipPersoana = tipPersoana,
                Telefon = "0711111111",
                Rol = rol
            };
        }

        private static User CreateUser(int id = 1, int persoanaId = 1, string username = "vendor")
        {
            return new User
            {
                IdUser = id,
                Username = username,
                Password = "pass",
                IdPersoana = persoanaId
            };
        }

        private static Preturi_Produs CreatePrice(
            int id = 1,
            int productId = 1,
            decimal price = 100,
            DateTime? start = null,
            DateTime? expiration = null,
            decimal? commission = null)
        {
            return new Preturi_Produs
            {
                idPP = id,
                IdProdus = productId,
                Pret = price,
                DataInceput = start ?? DateTime.UtcNow,
                DataExpirare = expiration,
                Comision = commission
            };
        }

        private static IFormFile CreateFakeImageFile()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5 };
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, bytes.Length, "Imagine", "test.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }

        private static ProductUploadDto CreateProductUploadDto(
            string category = "Vopsea",
            int userId = 1,
            IFormFile? image = null,
            string? colorCode = "RED-123",
            decimal price = 150)
        {
            return new ProductUploadDto
            {
                Nume = "Produs upload",
                Descriere = "Descriere upload",
                EsteSpray = true,
                Valabil = true,
                Categorie = category,
                IdUser = userId,
                Pret = price,
                Imagine = image,
                CodCuloare = colorCode
            };
        }

        private async Task SeedCategory()
        {
            _context.Categorii.Add(CreateCategory());
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserAndPerson(
            int userId = 1,
            int personId = 1,
            string username = "vendor",
            string role = "Participant",
            string tipPersoana = "Juridica")
        {
            _context.Persoane.Add(CreatePersoana(personId, "Vendor", "Test", tipPersoana, role));
            _context.Users.Add(CreateUser(userId, personId, username));

            await _context.SaveChangesAsync();
        }

        private async Task SeedBasicProductGraph()
        {
            await SeedCategory();
            await SeedUserAndPerson(1, 1, "vendor", "Participant", "Juridica");

            _context.Products.Add(CreateProduct(1, "Vopsea rosie", "Descriere", false, true, 1, 1));
            _context.Preturi_Produs.Add(CreatePrice(1, 1, 100, DateTime.UtcNow.AddDays(-2)));
            _context.Imagini.Add(new Imagini
            {
                IdProdus = 1,
                Fisier = new byte[] { 9, 9, 9 }
            });
            _context.Vopsele.Add(new Vopsea
            {
                IdProdus = 1,
                CodCuloare = "RED"
            });

            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task CreateProduct_ShouldPersistProduct()
        {
            var product = CreateProduct(0, "Vopsea rosie", "Descriere test", false, true, 1, 1);

            await _service.CreateProduct(product);

            ClassicAssert.AreEqual(1, _context.Products.Count(p => p.Nume == "Vopsea rosie"));
        }

        [Test]
        public async Task GetAll_WhenProductsExist_ShouldReturnAllProducts()
        {
            _context.Products.AddRange(
                CreateProduct(1, "Produs 1", "D1", true, true, 1, null),
                CreateProduct(2, "Produs 2", "D2", false, true, 1, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetAll();

            ClassicAssert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetAll_WhenNoProducts_ShouldReturnEmptyList()
        {
            var result = await _service.GetAll();

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetById_WhenProductExists_ShouldReturnProduct()
        {
            _context.Products.Add(CreateProduct(10, "Primer", "D", true, true, 1, null));
            await _context.SaveChangesAsync();

            var result = await _service.GetById(10);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("Primer", result!.Nume);
        }

        [Test]
        public async Task GetById_WhenProductDoesNotExist_ShouldReturnNull()
        {
            var result = await _service.GetById(999);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task SearchByName_ShouldReturnMatchingProducts()
        {
            _context.Products.AddRange(
                CreateProduct(1, "Vopsea albastra", "D", false, true, 1, null),
                CreateProduct(2, "Lac transparent", "D", false, true, 1, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.SearchByName("Vopsea");

            ClassicAssert.AreEqual(1, result.Count(p => p.Nume == "Vopsea albastra"));
        }

        [Test]
        public async Task SearchByName_WhenNoMatch_ShouldReturnEmptyList()
        {
            _context.Products.Add(CreateProduct(1, "Lac transparent", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            var result = await _service.SearchByName("Vopsea");

            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetByUserId_ShouldReturnOnlyUserProducts()
        {
            _context.Products.AddRange(
                CreateProduct(1, "Produs user 1", "D", false, true, 1, 1),
                CreateProduct(2, "Produs user 2", "D", false, true, 1, 2)
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetByUserId(1);

            ClassicAssert.AreEqual(1, result.Count(p => p.IdUser == 1));
        }

        [Test]
        public async Task GetByUserId_WhenUserHasNoProducts_ShouldReturnEmptyList()
        {
            var result = await _service.GetByUserId(999);

            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task UpdateProduct_ShouldModifyProduct()
        {
            _context.Products.Add(CreateProduct(20, "Vechi", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            await _service.UpdateProduct(CreateProduct(20, "Nou", "D nou", true, false, 1, null));

            var updated = await _context.Products.FindAsync(20);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("Nou", updated!.Nume);
            ClassicAssert.IsTrue(updated.EsteSpray);
            ClassicAssert.IsFalse(updated.Valabil);
        }

        [Test]
        public async Task DeleteProduct_WhenProductExists_ShouldRemoveProduct()
        {
            _context.Products.Add(CreateProduct(30, "De sters", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            await _service.DeleteProduct(30);

            var deleted = await _context.Products.FindAsync(30);

            ClassicAssert.IsNull(deleted);
        }

        [Test]
        public async Task DeleteProduct_WhenProductDoesNotExist_ShouldNotThrow()
        {
            await _service.DeleteProduct(999);

            ClassicAssert.AreEqual(0, _context.Products.Count());
        }

        [Test]
        public async Task GetByCategory_ShouldReturnProductsFromCategoryWithDetails()
        {
            await SeedBasicProductGraph();

            var result = await _service.GetByCategory("vopsea");

            ClassicAssert.AreEqual(1, result.Count(p => p.Nume == "Vopsea rosie"));

            var product = result.Single();

            ClassicAssert.AreEqual(100, product.Pret);
            ClassicAssert.AreEqual("Vopsea", product.Categorie);
            ClassicAssert.AreEqual("RED", product.CodCuloare);
            ClassicAssert.AreEqual(3, product.Imagine.Length);
        }

        [Test]
        public async Task GetByCategory_WhenProductHasNoPriceImageOrColor_ShouldUseFallbackValues()
        {
            await SeedCategory();

            _context.Products.Add(CreateProduct(1, "Simplu", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            var result = await _service.GetByCategory("Vopsea");

            var product = result.Single();

            ClassicAssert.AreEqual(0, product.Pret);
            ClassicAssert.AreEqual("N/A", product.CodCuloare);
            ClassicAssert.AreEqual(0, product.Imagine.Length);
        }

        [Test]
        public async Task GetByCategory_WhenNoProducts_ShouldReturnEmptyList()
        {
            await SeedCategory();

            var result = await _service.GetByCategory("Alta");

            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetByCategories_ShouldReturnProductsFromMultipleCategories()
        {
            _context.Categorii.AddRange(
                CreateCategory(1, "Vopsea", "D"),
                CreateCategory(2, "Spray", "D"),
                CreateCategory(3, "Accesorii", "D")
            );

            _context.Products.AddRange(
                CreateProduct(1, "Produs 1", "D", false, true, 1, null),
                CreateProduct(2, "Produs 2", "D", true, true, 2, null),
                CreateProduct(3, "Produs 3", "D", false, true, 3, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetByCategories(new List<string> { "vopsea", "spray" });

            ClassicAssert.AreEqual(2, result.Count());
            ClassicAssert.IsTrue(result.Any(p => p.Nume == "Produs 1"));
            ClassicAssert.IsTrue(result.Any(p => p.Nume == "Produs 2"));
        }

        [Test]
        public async Task GetProdusDTOsAsync_ShouldReturnDetailedProductsForValidVendors()
        {
            await SeedCategory();
            await SeedUserAndPerson(1, 1, "admin", "Administrator", "Juridica");
            await SeedUserAndPerson(2, 2, "vendor2", "Participant", "Juridica");

            _context.Products.AddRange(
                CreateProduct(1, " Produs admin ", " Descriere admin ", true, true, 1, 1),
                CreateProduct(2, " Produs vendor ", " Descriere vendor ", false, true, 1, 2),
                CreateProduct(3, "Invalid fara user", "D", false, true, 1, null),
                CreateProduct(4, "Invalid nevalabil", "D", false, false, 1, 2)
            );

            _context.Preturi_Produs.AddRange(
                CreatePrice(1, 1, 100, DateTime.UtcNow.AddDays(-2)),
                CreatePrice(2, 1, 120, DateTime.UtcNow.AddDays(-1)),
                CreatePrice(3, 2, 55, DateTime.UtcNow.AddDays(-1))
            );

            _context.Imagini.Add(new Imagini
            {
                IdProdus = 1,
                Fisier = new byte[] { 1, 2, 3 }
            });

            await _context.SaveChangesAsync();

            var result = (await _service.GetProdusDTOsAsync()).ToList();

            ClassicAssert.AreEqual(2, result.Count);

            var adminProduct = result.Single(p => p.ProdusId == 1);
            var vendorProduct = result.Single(p => p.ProdusId == 2);

            ClassicAssert.AreEqual("Produs admin", adminProduct.Nume);
            ClassicAssert.AreEqual("Descriere admin", adminProduct.Descriere);
            ClassicAssert.AreEqual("DA", adminProduct.EsteSprayText);
            ClassicAssert.AreEqual(120, adminProduct.Pret);
            ClassicAssert.AreEqual("SC AUTO PAINTS SRL", adminProduct.Vendor);
            ClassicAssert.IsNotNull(adminProduct.Imagine);

            ClassicAssert.AreEqual("NU", vendorProduct.EsteSprayText);
            ClassicAssert.AreEqual("Vendor Test", vendorProduct.Vendor);
        }

        [Test]
        public async Task GetProdusDTOsAsync_WhenVendorIsPhysicalPerson_ShouldThrowInvalidOperationException()
        {
            await SeedCategory();
            await SeedUserAndPerson(1, 1, "vendor", "Participant", "Fizica");

            _context.Products.Add(CreateProduct(1, "Produs", "D", false, true, 1, 1));
            await _context.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _service.GetProdusDTOsAsync();
            });
        }

        [Test]
        public async Task GetOfferedProductsAsync_ShouldReturnOnlyProductsWithActiveOffer()
        {
            await SeedCategory();
            await SeedUserAndPerson(1, 1, "admin", "Administrator", "Juridica");
            await SeedUserAndPerson(2, 2, "vendor2", "Participant", "Juridica");

            _context.Products.AddRange(
                CreateProduct(1, "Produs admin", "D", true, true, 1, 1),
                CreateProduct(2, "Produs vendor", "D", false, true, 1, 2),
                CreateProduct(3, "Produs fara oferta activa", "D", false, true, 1, 2)
            );

            _context.Preturi_Produs.AddRange(
                CreatePrice(1, 1, 70, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(5)),
                CreatePrice(2, 2, 80, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(5)),
                CreatePrice(3, 3, 90, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1))
            );

            await _context.SaveChangesAsync();

            var result = (await _service.GetOfferedProductsAsync()).ToList();

            ClassicAssert.AreEqual(2, result.Count);
            ClassicAssert.AreEqual("SC AUTOPAINTS SRL", result.Single(p => p.ProdusId == 1).Vendor);
            ClassicAssert.AreEqual("Vendor Test", result.Single(p => p.ProdusId == 2).Vendor);
        }

        [Test]
        public async Task GetOfferedProductsAsync_WhenNoActiveOffers_ShouldReturnEmptyList()
        {
            await SeedCategory();

            _context.Products.Add(CreateProduct(1, "Produs", "D", false, true, 1, null));
            _context.Preturi_Produs.Add(CreatePrice(1, 1, 100, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1)));

            await _context.SaveChangesAsync();

            var result = await _service.GetOfferedProductsAsync();

            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetProductsResponse_ShouldReturnOkWithProducts()
        {
            _context.Products.Add(CreateProduct(1, "Produs", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            var result = await _service.GetProductsResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.IsInstanceOf<IEnumerable<Produs>>(ok.Value);
        }

        [Test]
        public async Task GetProductsByCategoryResponse_WhenProductsExist_ShouldReturnOk()
        {
            await SeedBasicProductGraph();

            var result = await _service.GetProductsByCategoryResponse("Vopsea");

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetProductsByCategoryResponse_WhenNoProducts_ShouldReturnNotFound()
        {
            await SeedCategory();

            var result = await _service.GetProductsByCategoryResponse("Alta");

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("No products found in this category.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task GetProductsByCategoriesResponse_WhenCategoriesIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.GetProductsByCategoriesResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Please provide at least one category.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task GetProductsByCategoriesResponse_WhenCategoriesIsEmpty_ShouldReturnBadRequest()
        {
            var result = await _service.GetProductsByCategoriesResponse(new List<string>());

            ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task GetProductsByCategoriesResponse_WhenNoProducts_ShouldReturnNotFound()
        {
            await SeedCategory();

            var result = await _service.GetProductsByCategoriesResponse(new List<string> { "Alta" });

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("No products found in the specified categories.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task GetProductsByCategoriesResponse_WhenProductsExist_ShouldReturnOk()
        {
            await SeedBasicProductGraph();

            var result = await _service.GetProductsByCategoriesResponse(new List<string> { " Vopsea " });

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetProductsByUserIdResponse_WhenProductsExist_ShouldReturnOk()
        {
            _context.Products.Add(CreateProduct(1, "Produs user", "D", false, true, 1, 1));
            await _context.SaveChangesAsync();

            var result = await _service.GetProductsByUserIdResponse(1);

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetProductsByUserIdResponse_WhenNoProducts_ShouldReturnNotFound()
        {
            var result = await _service.GetProductsByUserIdResponse(1);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("No products found for this user.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task GetProductResponse_WhenProductExists_ShouldReturnOk()
        {
            _context.Products.Add(CreateProduct(1, "Produs", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            var result = await _service.GetProductResponse(1);

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetProductResponse_WhenProductMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetProductResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Product not found.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task SearchProductsByNameResponse_WhenNameIsEmpty_ShouldReturnBadRequest()
        {
            var result = await _service.SearchProductsByNameResponse("   ");

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Search term cannot be empty.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task SearchProductsByNameResponse_WhenNoProductsMatch_ShouldReturnNotFound()
        {
            _context.Products.Add(CreateProduct(1, "Lac", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            var result = await _service.SearchProductsByNameResponse("Vopsea");

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("No products match the search criteria.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task SearchProductsByNameResponse_WhenProductsMatch_ShouldReturnOk()
        {
            _context.Products.Add(CreateProduct(1, "Vopsea", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            var result = await _service.SearchProductsByNameResponse("Vopsea");

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task CreateProductResponse_WhenCategoryMissing_ShouldReturnBadRequest()
        {
            var dto = CreateProductUploadDto(category: "Inexistenta");

            var result = await _service.CreateProductResponse(dto);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Categoria specificata nu exista.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task CreateProductResponse_WhenOwnerCreatesProduct_ShouldCreateProductWithoutCommissionWithImageAndColor()
        {
            await SeedCategory();

            var dto = CreateProductUploadDto(userId: 1, image: CreateFakeImageFile(), colorCode: "RED-123", price: 200);

            var result = await _service.CreateProductResponse(dto);

            var created = result as CreatedAtActionResult;

            ClassicAssert.IsNotNull(created);
            ClassicAssert.AreEqual("GetProduct", created!.ActionName);
            ClassicAssert.AreEqual("Product", created.ControllerName);
            ClassicAssert.AreEqual("Produs creat cu succes!", GetAnonymousProperty(created.Value!, "message"));

            var idProdus = (int)GetAnonymousProperty(created.Value!, "IdProdus")!;
            var product = await _context.Products.FindAsync(idProdus);
            var price = _context.Preturi_Produs.Single(p => p.IdProdus == idProdus);
            var image = _context.Imagini.Single(i => i.IdProdus == idProdus);
            var paint = _context.Vopsele.Single(v => v.IdProdus == idProdus);

            ClassicAssert.IsNotNull(product);
            ClassicAssert.AreEqual(200, price.Pret);
            ClassicAssert.IsNull(price.Comision);
            ClassicAssert.AreEqual(5, image.Fisier.Length);
            ClassicAssert.AreEqual("RED-123", paint.CodCuloare);
        }

        [Test]
        public async Task CreateProductResponse_WhenNonOwnerCreatesProduct_ShouldCreateProductWithCommissionWithoutImageOrColor()
        {
            await SeedCategory();

            var dto = CreateProductUploadDto(userId: 2, image: null, colorCode: null, price: 300);

            var result = await _service.CreateProductResponse(dto);

            var created = result as CreatedAtActionResult;

            ClassicAssert.IsNotNull(created);

            var idProdus = (int)GetAnonymousProperty(created!.Value!, "IdProdus")!;
            var price = _context.Preturi_Produs.Single(p => p.IdProdus == idProdus);

            ClassicAssert.AreEqual(300, price.Pret);
            ClassicAssert.AreEqual(10m, price.Comision);
            ClassicAssert.AreEqual(0, _context.Imagini.Count());
            ClassicAssert.AreEqual(0, _context.Vopsele.Count());
        }

        [Test]
        public async Task CreateProductResponse_WhenUnexpectedErrorOccurs_ShouldReturnInternalServerError()
        {
            _context.Dispose();

            var result = await _service.CreateProductResponse(CreateProductUploadDto());

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("Eroare la crearea produsului.", GetAnonymousProperty(objectResult.Value!, "message"));
        }

        [Test]
        public async Task UpdateProductResponse_WhenIdMismatch_ShouldReturnBadRequest()
        {
            var result = await _service.UpdateProductResponse(1, CreateProduct(2, "Produs", "D", false, true, 1, null));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Product ID mismatch.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task UpdateProductResponse_WhenProductMissing_ShouldReturnNotFound()
        {
            var result = await _service.UpdateProductResponse(1, CreateProduct(1, "Produs", "D", false, true, 1, null));

            ClassicAssert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task UpdateProductResponse_WhenValid_ShouldReturnNoContent()
        {
            _context.Products.Add(CreateProduct(1, "Old", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            var result = await _service.UpdateProductResponse(1, CreateProduct(1, "New", "D2", true, false, 1, null));

            ClassicAssert.IsInstanceOf<NoContentResult>(result);

            var updated = await _context.Products.FindAsync(1);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("New", updated!.Nume);
        }

        [Test]
        public async Task DeleteProductResponse_WhenProductMissing_ShouldReturnNotFound()
        {
            var result = await _service.DeleteProductResponse(999);

            ClassicAssert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task DeleteProductResponse_WhenValid_ShouldReturnNoContent()
        {
            _context.Products.Add(CreateProduct(1, "Produs", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            var result = await _service.DeleteProductResponse(1);

            ClassicAssert.IsInstanceOf<NoContentResult>(result);
            ClassicAssert.AreEqual(0, _context.Products.Count());
        }

        [Test]
        public async Task GetDetailedProductsResponse_WhenNoProducts_ShouldReturnNotFound()
        {
            var result = await _service.GetDetailedProductsResponse();

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("No products found.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task GetDetailedProductsResponse_WhenProductsExist_ShouldReturnOk()
        {
            await SeedCategory();
            await SeedUserAndPerson(1, 1, "admin", "Administrator", "Juridica");

            _context.Products.Add(CreateProduct(1, "Produs", "D", false, true, 1, 1));
            _context.Preturi_Produs.Add(CreatePrice(1, 1, 100, DateTime.UtcNow));

            await _context.SaveChangesAsync();

            var result = await _service.GetDetailedProductsResponse();

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetOfferedProductsResponse_WhenNoOffers_ShouldReturnNotFound()
        {
            var result = await _service.GetOfferedProductsResponse();

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("No products in offer.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task GetOfferedProductsResponse_WhenOffersExist_ShouldReturnOk()
        {
            await SeedCategory();

            _context.Products.Add(CreateProduct(1, "Produs", "D", false, true, 1, null));
            _context.Preturi_Produs.Add(CreatePrice(1, 1, 80, DateTime.UtcNow, DateTime.UtcNow.AddDays(5)));

            await _context.SaveChangesAsync();

            var result = await _service.GetOfferedProductsResponse();

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task AdaugaReducereResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.AdaugaReducereResponse(null!);

            ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task AdaugaReducereResponse_WhenInvalidValues_ShouldReturnBadRequest()
        {
            var result = await _service.AdaugaReducereResponse(new Reducere_DTO
            {
                IdProdus = 0,
                PretNou = 0
            });

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Date reducere invalide!", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task AdaugaReducereResponse_WhenProductMissing_ShouldReturnNotFound()
        {
            var result = await _service.AdaugaReducereResponse(new Reducere_DTO
            {
                IdProdus = 999,
                PretNou = 50
            });

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Produsul nu exista!", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task AdaugaReducereResponse_WhenActiveDiscountAlreadyExists_ShouldReturnBadRequest()
        {
            _context.Products.Add(CreateProduct(1, "Produs", "D", false, true, 1, null));
            _context.Preturi_Produs.Add(CreatePrice(1, 1, 80, DateTime.UtcNow, DateTime.UtcNow.AddDays(3)));
            await _context.SaveChangesAsync();

            var result = await _service.AdaugaReducereResponse(new Reducere_DTO
            {
                IdProdus = 1,
                PretNou = 70
            });

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Exista deja o reducere activa pentru acest produs!", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task AdaugaReducereResponse_WhenValidWithDefaultExpiration_ShouldAddDiscount()
        {
            _context.Products.Add(CreateProduct(1, "Produs", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            var result = await _service.AdaugaReducereResponse(new Reducere_DTO
            {
                IdProdus = 1,
                PretNou = 70,
                DataExpirare = null
            });

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual("Reducere aplicata cu succes!", GetAnonymousProperty(ok!.Value!, "message"));
            ClassicAssert.AreEqual(1, _context.Preturi_Produs.Count());
            ClassicAssert.IsNotNull(_context.Preturi_Produs.Single().DataExpirare);
        }

        [Test]
        public async Task AdaugaReducereResponse_WhenValidWithCustomExpiration_ShouldAddDiscount()
        {
            _context.Products.Add(CreateProduct(1, "Produs", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            var expiration = DateTime.UtcNow.AddDays(2);

            var result = await _service.AdaugaReducereResponse(new Reducere_DTO
            {
                IdProdus = 1,
                PretNou = 60,
                DataExpirare = expiration
            });

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);
            ClassicAssert.AreEqual(expiration, _context.Preturi_Produs.Single().DataExpirare);
        }

        [Test]
        public async Task AfisareToatePreturileDinBDResponse_ShouldReturnOkWithPrices()
        {
            _context.Preturi_Produs.Add(CreatePrice(1, 1, 100, DateTime.UtcNow));
            await _context.SaveChangesAsync();

            var result = await _service.AfisareToatePreturileDinBDResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);

            var prices = AnonymousList(ok!.Value!);

            ClassicAssert.AreEqual(1, prices.Count);
            ClassicAssert.AreEqual(1, GetAnonymousProperty(prices[0], "IdProdus"));
        }

        [Test]
        public async Task AfiseazaReducereaResponse_WhenNoPrices_ShouldReturnNotFound()
        {
            var result = await _service.AfiseazaReducereaResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Nu exista preturi pentru acest produs.", notFound!.Value);
        }

        [Test]
        public async Task AfiseazaReducereaResponse_WhenOldAndNewPricesExist_ShouldReturnBoth()
        {
            _context.Preturi_Produs.AddRange(
                CreatePrice(1, 1, 100, DateTime.UtcNow.AddDays(-5), null),
                CreatePrice(2, 1, 70, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(5))
            );

            await _context.SaveChangesAsync();

            var result = await _service.AfiseazaReducereaResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.IsNotNull(GetAnonymousProperty(ok!.Value!, "pretVechi"));
            ClassicAssert.IsNotNull(GetAnonymousProperty(ok.Value!, "pretNou"));
        }

        [Test]
        public async Task AfiseazaReducereaResponse_WhenOnlyOldPriceExists_ShouldReturnOldAndNullNew()
        {
            _context.Preturi_Produs.Add(CreatePrice(1, 1, 100, DateTime.UtcNow, null));
            await _context.SaveChangesAsync();

            var result = await _service.AfiseazaReducereaResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.IsNotNull(GetAnonymousProperty(ok!.Value!, "pretVechi"));
            ClassicAssert.IsNull(GetAnonymousProperty(ok.Value!, "pretNou"));
        }

        [Test]
        public async Task AfiseazaReducereaResponse_WhenOnlyNewPriceExists_ShouldReturnNullOldAndNew()
        {
            _context.Preturi_Produs.Add(CreatePrice(1, 1, 70, DateTime.UtcNow, DateTime.UtcNow.AddDays(5)));
            await _context.SaveChangesAsync();

            var result = await _service.AfiseazaReducereaResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.IsNull(GetAnonymousProperty(ok!.Value!, "pretVechi"));
            ClassicAssert.IsNotNull(GetAnonymousProperty(ok.Value!, "pretNou"));
        }

        [Test]
        public async Task GetProduseCuReducereResponse_WhenDiscountedProductsExist_ShouldReturnProducts()
        {
            await SeedCategory();
            await SeedUserAndPerson(1, 1, "owner", "Owner", "Juridica");
            await SeedUserAndPerson(2, 2, "vendor", "Participant", "Juridica");

            _context.Products.AddRange(
                CreateProduct(1, "Produs owner", "D", false, true, 1, 1),
                CreateProduct(2, "Produs vendor", "D", false, true, 1, 2),
                CreateProduct(3, "Produs necunoscut", "D", false, true, 1, 999),
                CreateProduct(4, "Produs fara reducere reala", "D", false, true, 1, 2)
            );

            _context.Preturi_Produs.AddRange(
                CreatePrice(1, 1, 100, DateTime.UtcNow.AddDays(-5), null),
                CreatePrice(2, 1, 70, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(5)),
                CreatePrice(3, 2, 200, DateTime.UtcNow.AddDays(-5), null),
                CreatePrice(4, 2, 150, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(5)),
                CreatePrice(5, 3, 90, DateTime.UtcNow.AddDays(-5), null),
                CreatePrice(6, 3, 60, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(5)),
                CreatePrice(7, 4, 100, DateTime.UtcNow.AddDays(-5), null),
                CreatePrice(8, 4, 120, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(5))
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetProduseCuReducereResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);

            var products = AnonymousList(ok!.Value!);

            ClassicAssert.AreEqual(3, products.Count);
            ClassicAssert.IsTrue(products.Any(p => (string)GetAnonymousProperty(p, "Vendor")! == "SC AUTO PAINTS SRL"));
            ClassicAssert.IsTrue(products.Any(p => (string)GetAnonymousProperty(p, "Vendor")! == "Vendor Test"));
            ClassicAssert.IsTrue(products.Any(p => (string)GetAnonymousProperty(p, "Vendor")! == "Necunoscut"));
        }

        [Test]
        public async Task GetProduseCuReducereResponse_WhenNoDiscounts_ShouldReturnOkWithEmptyList()
        {
            var result = await _service.GetProduseCuReducereResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(0, AnonymousList(ok!.Value!).Count);
        }

        [Test]
        public async Task AdminUpdateProdusResponse_WhenProductMissing_ShouldReturnNotFound()
        {
            var result = await _service.AdminUpdateProdusResponse(999, new AdminUpdateProdus_DTO());

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Produsul nu exista.", notFound!.Value);
        }

        [Test]
        public async Task AdminUpdateProdusResponse_WhenAllFieldsProvided_ShouldUpdateProductCreateCategoryImageAndIncreaseQuantity()
        {
            _context.Categorii.Add(CreateCategory(1, "Old", "D"));
            _context.Products.Add(CreateProduct(1, "Old", "Old", false, true, 1, 1));
            _context.SubProduse.Add(new Subprodus
            {
                IdSubprodus = 1,
                IdProdus = 1,
                Valabil = true,
                idCos = null
            });

            await _context.SaveChangesAsync();

            var base64 = "data:image/png;base64," + Convert.ToBase64String(new byte[] { 1, 2, 3 });

            var result = await _service.AdminUpdateProdusResponse(1, new AdminUpdateProdus_DTO
            {
                Nume = "New",
                Descriere = "New desc",
                EsteSpray = true,
                Valabil = false,
                IdUser = 2,
                DenumireCategorie = "NouaCategorie",
                ImagineBase64 = base64,
                Cantitate = 3
            });

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual("Produsul a fost actualizat cu succes!", GetAnonymousProperty(ok!.Value!, "message"));

            var product = await _context.Products.FindAsync(1);

            ClassicAssert.IsNotNull(product);
            ClassicAssert.AreEqual("New", product!.Nume);
            ClassicAssert.AreEqual("New desc", product.Descriere);
            ClassicAssert.IsTrue(product.EsteSpray);
            ClassicAssert.IsFalse(product.Valabil);
            ClassicAssert.AreEqual(2, product.IdUser);
            ClassicAssert.AreEqual("nouacategorie", _context.Categorii.Single(c => c.IdCategorie == product.IdCategorie).DenumireCategorie);
            ClassicAssert.AreEqual(1, _context.Imagini.Count(i => i.IdProdus == 1));
            ClassicAssert.AreEqual(3, _context.SubProduse.Count(sp => sp.IdProdus == 1 && sp.Valabil));
        }

        [Test]
        public async Task AdminUpdateProdusResponse_WhenExistingCategoryImageAndLowerQuantity_ShouldUpdateImageAndDeactivateSubproducts()
        {
            _context.Categorii.AddRange(
                CreateCategory(1, "Old", "D"),
                CreateCategory(2, "Vopsea", "D")
            );

            _context.Products.Add(CreateProduct(1, "Old", "Old", false, true, 1, 1));

            _context.Imagini.Add(new Imagini
            {
                IdProdus = 1,
                Fisier = new byte[] { 9 }
            });

            _context.SubProduse.AddRange(
                new Subprodus { IdSubprodus = 1, IdProdus = 1, Valabil = true, idCos = null },
                new Subprodus { IdSubprodus = 2, IdProdus = 1, Valabil = true, idCos = null },
                new Subprodus { IdSubprodus = 3, IdProdus = 1, Valabil = true, idCos = null }
            );

            await _context.SaveChangesAsync();

            var base64 = Convert.ToBase64String(new byte[] { 7, 8 });

            var result = await _service.AdminUpdateProdusResponse(1, new AdminUpdateProdus_DTO
            {
                DenumireCategorie = "Vopsea",
                ImagineBase64 = base64,
                Cantitate = 1
            });

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var product = await _context.Products.FindAsync(1);
            var image = _context.Imagini.Single(i => i.IdProdus == 1);

            ClassicAssert.IsNotNull(product);
            ClassicAssert.AreEqual(2, product!.IdCategorie);
            ClassicAssert.AreEqual(2, image.Fisier.Length);
            ClassicAssert.AreEqual(1, _context.SubProduse.Count(sp => sp.IdProdus == 1 && sp.Valabil));
            ClassicAssert.AreEqual(2, _context.SubProduse.Count(sp => sp.IdProdus == 1 && !sp.Valabil));
        }

        [Test]
        public async Task AdminUpdateProdusResponse_WhenDtoHasEmptyFields_ShouldKeepExistingValues()
        {
            _context.Categorii.Add(CreateCategory(1, "Old", "D"));
            _context.Products.Add(CreateProduct(1, "Old", "Old desc", false, true, 1, 1));
            await _context.SaveChangesAsync();

            var result = await _service.AdminUpdateProdusResponse(1, new AdminUpdateProdus_DTO
            {
                Nume = "   ",
                Descriere = "",
                DenumireCategorie = "   "
            });

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var product = await _context.Products.FindAsync(1);

            ClassicAssert.IsNotNull(product);
            ClassicAssert.AreEqual("Old", product!.Nume);
            ClassicAssert.AreEqual("Old desc", product.Descriere);
            ClassicAssert.AreEqual(1, product.IdCategorie);
        }

        [Test]
        public async Task StergePretResponse_WhenPriceMissing_ShouldReturnNotFound()
        {
            var result = await _service.StergePretResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Pretul nu a fost gasit.", GetAnonymousProperty(notFound!.Value!, "mesaj"));
        }

        [Test]
        public async Task StergePretResponse_WhenPriceExists_ShouldDeletePrice()
        {
            _context.Preturi_Produs.Add(CreatePrice(1, 1, 100, DateTime.UtcNow));
            await _context.SaveChangesAsync();

            var result = await _service.StergePretResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual("Pretul a fost sters cu succes.", GetAnonymousProperty(ok!.Value!, "mesaj"));
            ClassicAssert.AreEqual(0, _context.Preturi_Produs.Count());
        }

        [Test]
        public async Task GetPreturiPentruProdusResponse_WhenNoPrices_ShouldReturnNotFound()
        {
            var result = await _service.GetPreturiPentruProdusResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Nu exista preturi pentru acest produs.", GetAnonymousProperty(notFound!.Value!, "mesaj"));
        }

        [Test]
        public async Task GetPreturiPentruProdusResponse_WhenPricesExist_ShouldReturnOk()
        {
            _context.Preturi_Produs.AddRange(
                CreatePrice(1, 1, 100, DateTime.UtcNow.AddDays(-2)),
                CreatePrice(2, 1, 90, DateTime.UtcNow.AddDays(-1))
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetPreturiPentruProdusResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);

            var prices = ok!.Value as IEnumerable<Preturi_Produs>;

            ClassicAssert.IsNotNull(prices);
            ClassicAssert.AreEqual(2, prices!.Count());
            ClassicAssert.AreEqual(2, prices.First().idPP);
        }

        [Test]
        public async Task ModificaPretResponse_WhenPriceMissing_ShouldReturnNotFound()
        {
            var result = await _service.ModificaPretResponse(999, new UpdatePret_DTO
            {
                Pret = 50
            });

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Pretul nu a fost gasit.", GetAnonymousProperty(notFound!.Value!, "mesaj"));
        }

        [Test]
        public async Task ModificaPretResponse_WhenOnlyPriceProvided_ShouldUpdateOnlyPrice()
        {
            _context.Preturi_Produs.Add(CreatePrice(1, 1, 100, DateTime.UtcNow, null, null));
            await _context.SaveChangesAsync();

            var result = await _service.ModificaPretResponse(1, new UpdatePret_DTO
            {
                Pret = 80
            });

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var price = await _context.Preturi_Produs.FindAsync(1);

            ClassicAssert.IsNotNull(price);
            ClassicAssert.AreEqual(80, price!.Pret);
            ClassicAssert.IsNull(price.Comision);
            ClassicAssert.IsNull(price.DataExpirare);
        }

        [Test]
        public async Task ModificaPretResponse_WhenAllOptionalFieldsProvided_ShouldUpdateAll()
        {
            _context.Preturi_Produs.Add(CreatePrice(1, 1, 100, DateTime.UtcNow, null, null));
            await _context.SaveChangesAsync();

            var expiration = DateTime.UtcNow.AddDays(4);

            var result = await _service.ModificaPretResponse(1, new UpdatePret_DTO
            {
                Pret = 75,
                Comision = 15,
                DataExpirare = expiration
            });

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual("Pretul a fost actualizat cu succes.", GetAnonymousProperty(ok!.Value!, "mesaj"));

            var price = await _context.Preturi_Produs.FindAsync(1);

            ClassicAssert.IsNotNull(price);
            ClassicAssert.AreEqual(75, price!.Pret);
            ClassicAssert.AreEqual(15, price.Comision);
            ClassicAssert.AreEqual(expiration, price.DataExpirare);
        }

        [Test]
        public async Task DezactiveazaProdusResponse_WhenProductMissing_ShouldReturnNotFound()
        {
            var result = await _service.DezactiveazaProdusResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Produsul cu ID 999 nu a fost gasit.", notFound!.Value);
        }

        [Test]
        public async Task DezactiveazaProdusResponse_WhenProductExists_ShouldSetValabilFalse()
        {
            _context.Products.Add(CreateProduct(1, "Produs", "D", false, true, 1, null));
            await _context.SaveChangesAsync();

            var result = await _service.DezactiveazaProdusResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual("Produsul a fost dezactivat cu succes.", GetAnonymousProperty(ok!.Value!, "message"));

            var product = await _context.Products.FindAsync(1);

            ClassicAssert.IsNotNull(product);
            ClassicAssert.IsFalse(product!.Valabil);
        }
    }
}