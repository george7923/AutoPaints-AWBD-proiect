using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.UnitTests
{
    [TestFixture]
    public class SubprodusServiceTests
    {
        private ApplicationDbContext _context = null!;
        private SubprodusService _service = null!;

        [SetUp]
        public void Before()
        {
            _context = TestDbContextFactory.CreateContext();
            _service = new SubprodusService(_context);
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

        private static Subprodus CreateSubprodus(
            int id = 1,
            int idProdus = 1,
            bool valabil = true,
            int? idCos = null)
        {
            return new Subprodus
            {
                IdSubprodus = id,
                IdProdus = idProdus,
                Valabil = valabil,
                idCos = idCos
            };
        }

        private static Cos CreateCos(int idCos = 1, int idUser = 1)
        {
            return new Cos
            {
                idCos = idCos,
                CodUnic = $"COS-{idCos}",
                IdUser = idUser,
                DataCreare = DateTime.UtcNow
            };
        }

        private static Categorii CreateCategory(int id = 1, string name = "Vopsea")
        {
            return new Categorii
            {
                IdCategorie = id,
                DenumireCategorie = name,
                DescriereCategorie = "Categorie test"
            };
        }

        private static Produs CreateProduct(
            int id = 1,
            string name = "Produs test",
            int categoryId = 1,
            bool esteSpray = false,
            bool valabil = true)
        {
            return new Produs
            {
                IdProdus = id,
                Nume = name,
                Descriere = "Descriere test",
                EsteSpray = esteSpray,
                Valabil = valabil,
                IdCategorie = categoryId
            };
        }

        private static Preturi_Produs CreatePrice(
            int id = 1,
            int productId = 1,
            decimal price = 100,
            DateTime? start = null)
        {
            return new Preturi_Produs
            {
                idPP = id,
                IdProdus = productId,
                Pret = price,
                DataInceput = start ?? DateTime.UtcNow
            };
        }

        private async Task SeedProductCartGraph()
        {
            _context.Cosuri.Add(CreateCos(1, 1));

            _context.Categorii.Add(CreateCategory(1, "Vopsea"));

            _context.Products.AddRange(
                CreateProduct(1, "Produs 1", 1, false, true),
                CreateProduct(2, "Produs 2", 1, true, true),
                CreateProduct(3, "Produs fara cos", 1, false, true)
            );

            _context.Preturi_Produs.AddRange(
                CreatePrice(1, 1, 50, DateTime.UtcNow.AddDays(-5)),
                CreatePrice(2, 1, 75, DateTime.UtcNow.AddDays(-1)),
                CreatePrice(3, 2, 100, DateTime.UtcNow.AddDays(-1))
            );

            _context.Vopsele.Add(new Vopsea
            {
                IdProdus = 1,
                CodCuloare = "RED"
            });

            _context.Imagini.Add(new Imagini
            {
                IdProdus = 1,
                Fisier = new byte[] { 1, 2, 3 }
            });

            _context.SubProduse.AddRange(
                CreateSubprodus(1, 1, true, 1),
                CreateSubprodus(2, 1, true, 1),
                CreateSubprodus(3, 2, true, 1),
                CreateSubprodus(4, 3, true, null)
            );

            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task GetProdusByCosId_ShouldReturnNullBecauseMethodIsStub()
        {
            var result = await _service.GetProdusByCosId(1);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task CreateSubprodus_ShouldPersistSubprodus()
        {
            await _service.CreateSubprodus(new Subprodus
            {
                IdProdus = 1,
                Valabil = true,
                idCos = null
            });

            ClassicAssert.AreEqual(1, _context.SubProduse.Count(sp => sp.IdProdus == 1));
        }

        [Test]
        public async Task GetAllSubproduse_ShouldReturnAll()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 1, true, null),
                CreateSubprodus(2, 2, false, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetAllSubproduse();

            ClassicAssert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetAllSubproduse_WhenEmpty_ShouldReturnEmptyList()
        {
            var result = await _service.GetAllSubproduse();

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetSubprodusById_WhenExists_ShouldReturnSubprodus()
        {
            _context.SubProduse.Add(CreateSubprodus(10, 1, true, null));

            await _context.SaveChangesAsync();

            var result = await _service.GetSubprodusById(10);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(1, result!.IdProdus);
            ClassicAssert.IsTrue(result.Valabil);
        }

        [Test]
        public async Task GetSubprodusById_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetSubprodusById(999);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task DeleteSubprodus_ById_WhenExists_ShouldRemoveSubprodus()
        {
            _context.SubProduse.Add(CreateSubprodus(11, 1, true, null));

            await _context.SaveChangesAsync();

            await _service.DeleteSubprodus(11);

            var deleted = await _context.SubProduse.FindAsync(11);

            ClassicAssert.IsNull(deleted);
        }

        [Test]
        public async Task DeleteSubprodus_ById_WhenMissing_ShouldNotThrow()
        {
            await _service.DeleteSubprodus(999);

            ClassicAssert.AreEqual(0, _context.SubProduse.Count());
        }

        [Test]
        public async Task DeleteSubprodus_ByCosAndProduct_WhenExists_ShouldRemoveMatchingSubprodus()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(20, 1, true, 5),
                CreateSubprodus(21, 2, true, 5)
            );

            await _context.SaveChangesAsync();

            await _service.DeleteSubprodus(5, 1);

            ClassicAssert.IsFalse(_context.SubProduse.Any(sp => sp.IdSubprodus == 20));
            ClassicAssert.IsTrue(_context.SubProduse.Any(sp => sp.IdSubprodus == 21));
        }

        [Test]
        public async Task DeleteSubprodus_ByCosAndProduct_WhenMissing_ShouldNotThrow()
        {
            _context.SubProduse.Add(CreateSubprodus(21, 2, true, 5));

            await _context.SaveChangesAsync();

            await _service.DeleteSubprodus(999, 999);

            ClassicAssert.AreEqual(1, _context.SubProduse.Count());
        }

        [Test]
        public async Task DeleteSubproduseByProdusId_WhenExists_ShouldRemoveAllForProduct()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 100, true, null),
                CreateSubprodus(2, 100, true, null),
                CreateSubprodus(3, 200, true, null)
            );

            await _context.SaveChangesAsync();

            await _service.DeleteSubproduseByProdusId(100);

            ClassicAssert.AreEqual(0, _context.SubProduse.Count(sp => sp.IdProdus == 100));
            ClassicAssert.AreEqual(1, _context.SubProduse.Count(sp => sp.IdProdus == 200));
        }

        [Test]
        public async Task DeleteSubproduseByProdusId_WhenMissing_ShouldNotThrow()
        {
            _context.SubProduse.Add(CreateSubprodus(1, 100, true, null));

            await _context.SaveChangesAsync();

            await _service.DeleteSubproduseByProdusId(999);

            ClassicAssert.AreEqual(1, _context.SubProduse.Count());
        }

        [Test]
        public async Task DeleteSubproduseByCosId_WhenExists_ShouldRemoveAllForCart()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 1, true, 3),
                CreateSubprodus(2, 2, true, 3),
                CreateSubprodus(3, 3, true, 4)
            );

            await _context.SaveChangesAsync();

            await _service.DeleteSubproduseByCosId(3);

            ClassicAssert.AreEqual(0, _context.SubProduse.Count(sp => sp.idCos == 3));
            ClassicAssert.AreEqual(1, _context.SubProduse.Count(sp => sp.idCos == 4));
        }

        [Test]
        public async Task DeleteSubproduseByCosId_WhenMissing_ShouldNotThrow()
        {
            _context.SubProduse.Add(CreateSubprodus(1, 1, true, 3));

            await _context.SaveChangesAsync();

            await _service.DeleteSubproduseByCosId(999);

            ClassicAssert.AreEqual(1, _context.SubProduse.Count());
        }

        [Test]
        public async Task GetSubprodusByProdusId_WhenExists_ShouldReturnSubproduseWithCosIncluded()
        {
            _context.Cosuri.Add(CreateCos(1, 1));

            _context.SubProduse.AddRange(
                CreateSubprodus(1, 10, true, 1),
                CreateSubprodus(2, 10, false, null),
                CreateSubprodus(3, 20, true, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetSubprodusByProdusId(10);

            ClassicAssert.AreEqual(2, result.Count());

            var assigned = result.Single(sp => sp.IdSubprodus == 1);

            ClassicAssert.IsNotNull(assigned.Cos);
            ClassicAssert.AreEqual(1, assigned.Cos!.idCos);
        }

        [Test]
        public async Task GetSubprodusByProdusId_WhenMissing_ShouldReturnEmptyList()
        {
            var result = await _service.GetSubprodusByProdusId(999);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task IsSubprodusValabil_WhenValidSubprodusExists_ShouldReturnTrue()
        {
            _context.SubProduse.Add(CreateSubprodus(7, 1, true, null));

            await _context.SaveChangesAsync();

            var result = await _service.IsSubprodusValabil(7);

            ClassicAssert.IsTrue(result);
        }

        [Test]
        public async Task IsSubprodusValabil_WhenSubprodusIsNotValid_ShouldReturnFalse()
        {
            _context.SubProduse.Add(CreateSubprodus(7, 1, false, null));

            await _context.SaveChangesAsync();

            var result = await _service.IsSubprodusValabil(7);

            ClassicAssert.IsFalse(result);
        }

        [Test]
        public async Task IsSubprodusValabil_WhenSubprodusMissing_ShouldReturnFalse()
        {
            var result = await _service.IsSubprodusValabil(999);

            ClassicAssert.IsFalse(result);
        }

        [Test]
        public async Task CountSubproduseByProdusId_ShouldReturnCount()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 1, true, null),
                CreateSubprodus(2, 1, false, null),
                CreateSubprodus(3, 2, true, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.CountSubproduseByProdusId(1);

            ClassicAssert.AreEqual(2, result);
        }

        [Test]
        public async Task CountSubproduseByProdusId_WhenNoneExist_ShouldReturnZero()
        {
            var result = await _service.CountSubproduseByProdusId(999);

            ClassicAssert.AreEqual(0, result);
        }

        [Test]
        public async Task CountAvailableSubproduseByProdusId_ShouldCountOnlyAvailableAndUnassigned()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 1, true, null),
                CreateSubprodus(2, 1, true, 1),
                CreateSubprodus(3, 1, false, null),
                CreateSubprodus(4, 2, true, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.CountAvailableSubproduseByProdusId(1);

            ClassicAssert.AreEqual(1, result);
        }

        [Test]
        public async Task CountAvailableSubproduseByProdusId_WhenNoneAvailable_ShouldReturnZero()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 1, false, null),
                CreateSubprodus(2, 1, true, 3)
            );

            await _context.SaveChangesAsync();

            var result = await _service.CountAvailableSubproduseByProdusId(1);

            ClassicAssert.AreEqual(0, result);
        }

        [Test]
        public async Task GetAvailableSubproduse_ShouldRespectQuantity()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 1, true, null),
                CreateSubprodus(2, 1, true, null),
                CreateSubprodus(3, 1, true, null),
                CreateSubprodus(4, 1, true, 2),
                CreateSubprodus(5, 2, true, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetAvailableSubproduse(1, 2);

            ClassicAssert.AreEqual(2, result.Count);
            ClassicAssert.IsTrue(result.All(sp => sp.IdProdus == 1 && sp.idCos == null));
        }

        [Test]
        public async Task GetAvailableSubproduse_WhenQuantityGreaterThanAvailable_ShouldReturnAllAvailable()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 1, true, null),
                CreateSubprodus(2, 1, true, 2)
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetAvailableSubproduse(1, 10);

            ClassicAssert.AreEqual(1, result.Count);
        }

        [Test]
        public async Task UpdateSubproduse_ShouldPersistChanges()
        {
            _context.SubProduse.Add(CreateSubprodus(88, 1, true, null));

            await _context.SaveChangesAsync();

            var sp = _context.SubProduse.Single();
            sp.Valabil = false;
            sp.idCos = 5;

            await _service.UpdateSubproduse(new List<Subprodus> { sp });

            var updated = await _context.SubProduse.FindAsync(88);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.IsFalse(updated!.Valabil);
            ClassicAssert.AreEqual(5, updated.idCos);
        }

        [Test]
        public async Task GetProdusByCos_JoinLaToateProdusele_WhenProductsExist_ShouldReturnGroupedProductsWithDetails()
        {
            await SeedProductCartGraph();

            var result = (await _service.GetProdusByCos_JoinLaToateProdusele(1)).ToList();

            ClassicAssert.AreEqual(2, result.Count);

            var productOne = result.Single(p => p.IdProdus == 1);
            var productTwo = result.Single(p => p.IdProdus == 2);

            ClassicAssert.AreEqual("Produs 1", productOne.Nume);
            ClassicAssert.AreEqual(75, productOne.Pret);
            ClassicAssert.AreEqual("Vopsea", productOne.Categorie);
            ClassicAssert.IsFalse(productOne.EsteSpray);
            ClassicAssert.AreEqual("RED", productOne.CodCuloare);
            ClassicAssert.AreEqual(3, productOne.Imagine.Length);
            ClassicAssert.AreEqual(2, productOne.Cantitatea);

            ClassicAssert.AreEqual("Produs 2", productTwo.Nume);
            ClassicAssert.AreEqual(100, productTwo.Pret);
            ClassicAssert.IsTrue(productTwo.EsteSpray);
            ClassicAssert.AreEqual("N/A", productTwo.CodCuloare);
            ClassicAssert.AreEqual(0, productTwo.Imagine.Length);
            ClassicAssert.AreEqual(1, productTwo.Cantitatea);
        }

        [Test]
        public async Task GetProdusByCos_JoinLaToateProdusele_WhenNoProductsExist_ShouldReturnEmptyList()
        {
            var result = await _service.GetProdusByCos_JoinLaToateProdusele(999);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetProdusByCos_JoinLaToateProdusele_WhenProductHasNoPrice_ShouldUseZeroPrice()
        {
            _context.Cosuri.Add(CreateCos(1, 1));
            _context.Categorii.Add(CreateCategory(1, "Vopsea"));
            _context.Products.Add(CreateProduct(1, "Fara pret", 1, false, true));
            _context.SubProduse.Add(CreateSubprodus(1, 1, true, 1));

            await _context.SaveChangesAsync();

            var result = (await _service.GetProdusByCos_JoinLaToateProdusele(1)).ToList();

            ClassicAssert.AreEqual(1, result.Count);
            ClassicAssert.AreEqual(0, result.Single().Pret);
            ClassicAssert.AreEqual("N/A", result.Single().CodCuloare);
            ClassicAssert.AreEqual(0, result.Single().Imagine.Length);
        }

        [Test]
        public async Task GetAllSubproduseResponse_ShouldReturnOkWithSubproduse()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 1, true, null),
                CreateSubprodus(2, 2, false, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetAllSubproduseResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var value = ok.Value as IEnumerable<Subprodus>;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.AreEqual(2, value!.Count());
        }

        [Test]
        public async Task GetSubprodusByIdResponse_WhenSubprodusExists_ShouldReturnOk()
        {
            _context.SubProduse.Add(CreateSubprodus(1, 1, true, null));

            await _context.SaveChangesAsync();

            var result = await _service.GetSubprodusByIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.IsInstanceOf<Subprodus>(ok.Value);

            var value = ok.Value as Subprodus;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.AreEqual(1, value!.IdSubprodus);
        }

        [Test]
        public async Task GetSubprodusByIdResponse_WhenSubprodusMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetSubprodusByIdResponse(999);

            ClassicAssert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task CreateSubprodusResponse_WhenModelStateIsInvalid_ShouldReturnBadRequestWithErrors()
        {
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("IdProdus", "IdProdus este obligatoriu.");

            var result = await _service.CreateSubprodusResponse(CreateSubprodus(1, 1, true, null), modelState);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Model invalid", GetAnonymousProperty(badRequest.Value!, "message"));

            var details = GetAnonymousProperty(badRequest.Value!, "details") as IEnumerable<string>;

            ClassicAssert.IsNotNull(details);
            ClassicAssert.IsTrue(details!.Contains("IdProdus este obligatoriu."));
        }

        [Test]
        public async Task CreateSubprodusResponse_WhenValid_ShouldReturnCreatedAtActionAndPersist()
        {
            var modelState = new ModelStateDictionary();

            var subprodus = new Subprodus
            {
                IdProdus = 1,
                Valabil = true,
                idCos = null,
                Produs = CreateProduct(1),
                Cos = CreateCos(1)
            };

            var result = await _service.CreateSubprodusResponse(subprodus, modelState);

            var created = result as CreatedAtActionResult;

            ClassicAssert.IsNotNull(created);
            ClassicAssert.AreEqual(201, created!.StatusCode);
            ClassicAssert.AreEqual("GetSubprodusById", created.ActionName);
            ClassicAssert.AreEqual("Subprodus", created.ControllerName);
            ClassicAssert.IsInstanceOf<Subprodus>(created.Value);

            var value = created.Value as Subprodus;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.IsTrue(value!.IdSubprodus > 0);
            ClassicAssert.IsNull(value.Produs);
            ClassicAssert.IsNull(value.Cos);
            ClassicAssert.AreEqual(1, _context.SubProduse.Count());
        }

        [Test]
        public async Task CreateSubprodusResponse_WhenCreateThrows_ShouldReturnBadRequest()
        {
            var modelState = new ModelStateDictionary();

            _context.Dispose();

            var result = await _service.CreateSubprodusResponse(CreateSubprodus(1, 1, true, null), modelState);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.IsNotNull(GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task DeleteSubprodusResponse_WhenSubprodusExists_ShouldReturnNoContentAndDelete()
        {
            _context.SubProduse.Add(CreateSubprodus(1, 1, true, null));

            await _context.SaveChangesAsync();

            var result = await _service.DeleteSubprodusResponse(1);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
            ClassicAssert.AreEqual(0, _context.SubProduse.Count());
        }

        [Test]
        public async Task DeleteSubprodusResponse_WhenSubprodusMissing_ShouldReturnNoContent()
        {
            var result = await _service.DeleteSubprodusResponse(999);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
        }

        [Test]
        public async Task DeleteSubprodusByCosAndProdusResponse_WhenSubprodusExists_ShouldReturnNoContentAndDelete()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 10, true, 20),
                CreateSubprodus(2, 11, true, 20)
            );

            await _context.SaveChangesAsync();

            var result = await _service.DeleteSubprodusByCosAndProdusResponse(20, 10);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
            ClassicAssert.IsFalse(_context.SubProduse.Any(sp => sp.IdSubprodus == 1));
            ClassicAssert.IsTrue(_context.SubProduse.Any(sp => sp.IdSubprodus == 2));
        }

        [Test]
        public async Task DeleteSubprodusByCosAndProdusResponse_WhenSubprodusMissing_ShouldReturnNoContent()
        {
            _context.SubProduse.Add(CreateSubprodus(1, 10, true, 20));

            await _context.SaveChangesAsync();

            var result = await _service.DeleteSubprodusByCosAndProdusResponse(999, 999);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
            ClassicAssert.AreEqual(1, _context.SubProduse.Count());
        }

        [Test]
        public async Task DeleteSubprodusByCosAndProdusResponse_WhenUnexpectedErrorOccurs_ShouldReturnNotFound()
        {
            _context.Dispose();

            var result = await _service.DeleteSubprodusByCosAndProdusResponse(1, 1);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual(
                "Subprodus not found for the given idCos and IdProdus.",
                GetAnonymousProperty(notFound.Value!, "message")
            );
        }

        [Test]
        public async Task DeleteSubproduseByProdusIdResponse_WhenSubproduseExist_ShouldReturnNoContentAndDelete()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 10, true, null),
                CreateSubprodus(2, 10, true, null),
                CreateSubprodus(3, 11, true, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.DeleteSubproduseByProdusIdResponse(10);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
            ClassicAssert.AreEqual(0, _context.SubProduse.Count(sp => sp.IdProdus == 10));
            ClassicAssert.AreEqual(1, _context.SubProduse.Count(sp => sp.IdProdus == 11));
        }

        [Test]
        public async Task DeleteSubproduseByProdusIdResponse_WhenNoneExist_ShouldReturnNoContent()
        {
            var result = await _service.DeleteSubproduseByProdusIdResponse(999);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
        }

        [Test]
        public async Task DeleteSubproduseByProdusIdResponse_WhenUnexpectedErrorOccurs_ShouldReturnNotFound()
        {
            _context.Dispose();

            var result = await _service.DeleteSubproduseByProdusIdResponse(1);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual(
                "Error deleting Subproduse for the given IdProdus.",
                GetAnonymousProperty(notFound.Value!, "message")
            );
        }

        [Test]
        public async Task DeleteSubproduseByCosIdResponse_WhenSubproduseExist_ShouldReturnNoContentAndDelete()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 10, true, 5),
                CreateSubprodus(2, 11, true, 5),
                CreateSubprodus(3, 12, true, 6)
            );

            await _context.SaveChangesAsync();

            var result = await _service.DeleteSubproduseByCosIdResponse(5);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
            ClassicAssert.AreEqual(0, _context.SubProduse.Count(sp => sp.idCos == 5));
            ClassicAssert.AreEqual(1, _context.SubProduse.Count(sp => sp.idCos == 6));
        }

        [Test]
        public async Task DeleteSubproduseByCosIdResponse_WhenNoneExist_ShouldReturnNoContent()
        {
            var result = await _service.DeleteSubproduseByCosIdResponse(999);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
        }

        [Test]
        public async Task DeleteSubproduseByCosIdResponse_WhenUnexpectedErrorOccurs_ShouldReturnNotFound()
        {
            _context.Dispose();

            var result = await _service.DeleteSubproduseByCosIdResponse(1);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual(
                "Error deleting Subproduse for the given idCos.",
                GetAnonymousProperty(notFound.Value!, "message")
            );
        }

        [Test]
        public async Task GetSubprodusByProdusIdResponse_ShouldReturnOkWithSubproduse()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 10, true, null),
                CreateSubprodus(2, 10, false, null),
                CreateSubprodus(3, 11, true, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetSubprodusByProdusIdResponse(10);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var value = ok.Value as IEnumerable<Subprodus>;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.AreEqual(2, value!.Count());
        }

        [Test]
        public async Task CountSubproduseByProdusIdResponse_ShouldReturnOkWithCount()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 10, true, null),
                CreateSubprodus(2, 10, false, null),
                CreateSubprodus(3, 11, true, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.CountSubproduseByProdusIdResponse(10);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual(2, GetAnonymousProperty(ok.Value!, "Count"));
        }

        [Test]
        public async Task IsSubprodusValabilResponse_WhenValid_ShouldReturnOkWithTrue()
        {
            _context.SubProduse.Add(CreateSubprodus(1, 10, true, null));

            await _context.SaveChangesAsync();

            var result = await _service.IsSubprodusValabilResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual(true, GetAnonymousProperty(ok.Value!, "IsValabil"));
        }

        [Test]
        public async Task IsSubprodusValabilResponse_WhenMissing_ShouldReturnOkWithFalse()
        {
            var result = await _service.IsSubprodusValabilResponse(999);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual(false, GetAnonymousProperty(ok.Value!, "IsValabil"));
        }

        [Test]
        public async Task CountAvailableSubproduseByProdusIdResponse_ShouldReturnOkWithCount()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 10, true, null),
                CreateSubprodus(2, 10, true, null),
                CreateSubprodus(3, 10, false, null),
                CreateSubprodus(4, 10, true, 1)
            );

            await _context.SaveChangesAsync();

            var result = await _service.CountAvailableSubproduseByProdusIdResponse(10);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual(2, GetAnonymousProperty(ok.Value!, "Count"));
        }

        [Test]
        public async Task CountAvailableSubproduseResponse_ShouldReturnOkWithCount()
        {
            _context.SubProduse.AddRange(
                CreateSubprodus(1, 10, true, null),
                CreateSubprodus(2, 10, false, null),
                CreateSubprodus(3, 10, true, 1),
                CreateSubprodus(4, 10, true, null)
            );

            await _context.SaveChangesAsync();

            var result = await _service.CountAvailableSubproduseResponse(10);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual(2, ok.Value);
        }

        [Test]
        public async Task GetProdusByCos_JoinLaToateProduseleResponse_WhenProductsExist_ShouldReturnOk()
        {
            await SeedProductCartGraph();

            var result = await _service.GetProdusByCos_JoinLaToateProduseleResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var products = ok.Value as IEnumerable<LicentaInAngular.Server.DataLayer.DTO.ProdusCosDTO_2>;

            ClassicAssert.IsNotNull(products);
            ClassicAssert.AreEqual(2, products!.Count());
        }

        [Test]
        public async Task GetProdusByCos_JoinLaToateProduseleResponse_WhenNoProductsExist_ShouldReturnNotFound()
        {
            var result = await _service.GetProdusByCos_JoinLaToateProduseleResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("No products found for this Cos.", GetAnonymousProperty(notFound.Value!, "message"));
        }
    }
}