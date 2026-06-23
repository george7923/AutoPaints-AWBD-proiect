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
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.UnitTests
{
    [TestFixture]
    public class SubcomandaServiceTests
    {
        private ApplicationDbContext _context = null!;
        private SubcomandaService _service = null!;

        [SetUp]
        public void Before()
        {
            _context = TestDbContextFactory.CreateContext();
            _service = new SubcomandaService(_context);
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

        private static Produs CreateProduct(
            int id = 1,
            string name = "Produs",
            string description = "Descriere",
            bool esteSpray = false,
            bool valabil = true,
            int categoryId = 1)
        {
            return new Produs
            {
                IdProdus = id,
                Nume = name,
                Descriere = description,
                EsteSpray = esteSpray,
                Valabil = valabil,
                IdCategorie = categoryId
            };
        }

        private static Comanda CreateComanda(
            int id = 1,
            int userId = 1,
            int idAdresa = 1,
            double pretTotal = 100,
            bool isPlaced = false)
        {
            return new Comanda
            {
                IdComanda = id,
                IdUser = userId,
                IdAdresa = idAdresa,
                ETA = DateTime.UtcNow,
                IsPlaced = isPlaced,
                PretTotal = pretTotal
            };
        }

        private static Subcomanda CreateSubcomanda(
            int id = 1,
            int idProdus = 1,
            int idComanda = 1,
            int totalSubproduse = 1)
        {
            return new Subcomanda
            {
                IdSubcomanda = id,
                IdProdus = idProdus,
                IdComanda = idComanda,
                TotalSubproduse = totalSubproduse
            };
        }

        private async Task SeedProductsOrdersAndSuborders()
        {
            _context.Products.AddRange(
                CreateProduct(1, "Produs 1"),
                CreateProduct(2, "Produs 2", "Descriere 2", true),
                CreateProduct(3, "Produs fara subcomanda")
            );

            _context.Comenzi.AddRange(
                CreateComanda(1, userId: 10),
                CreateComanda(2, userId: 20)
            );

            _context.Subcomenzi.AddRange(
                CreateSubcomanda(1, idProdus: 1, idComanda: 1, totalSubproduse: 2),
                CreateSubcomanda(2, idProdus: 2, idComanda: 1, totalSubproduse: 1),
                CreateSubcomanda(3, idProdus: 1, idComanda: 2, totalSubproduse: 3)
            );

            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task AddSubcomanda_ShouldPersistSubcomanda()
        {
            await _service.AddSubcomanda(new Subcomanda
            {
                IdProdus = 1,
                IdComanda = 1,
                TotalSubproduse = 2
            });

            ClassicAssert.AreEqual(1, _context.Subcomenzi.Count(sc => sc.TotalSubproduse == 2));
        }

        [Test]
        public async Task GetSubcomandaById_WhenExists_ShouldReturnSubcomanda()
        {
            _context.Subcomenzi.Add(CreateSubcomanda(5, 1, 1, 4));
            await _context.SaveChangesAsync();

            var result = await _service.GetSubcomandaById(5);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(5, result!.IdSubcomanda);
            ClassicAssert.AreEqual(4, result.TotalSubproduse);
        }

        [Test]
        public async Task GetSubcomandaById_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetSubcomandaById(999);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task GetAllSubcomenzi_WhenSubcomenziExist_ShouldReturnAllWithIncludes()
        {
            await SeedProductsOrdersAndSuborders();

            var result = await _service.GetAllSubcomenzi();

            ClassicAssert.AreEqual(3, result.Count());

            var first = result.First(sc => sc.IdSubcomanda == 1);

            ClassicAssert.IsNotNull(first.Produs);
            ClassicAssert.IsNotNull(first.Comanda);
            ClassicAssert.AreEqual("Produs 1", first.Produs!.Nume);
            ClassicAssert.AreEqual(10, first.Comanda!.IdUser);
        }

        [Test]
        public async Task GetAllSubcomenzi_WhenEmpty_ShouldReturnEmptyList()
        {
            var result = await _service.GetAllSubcomenzi();

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetProdusByComandaId_WhenProductsExist_ShouldReturnProductsForOrder()
        {
            await SeedProductsOrdersAndSuborders();

            var result = await _service.GetProdusByComandaId(1);

            ClassicAssert.AreEqual(2, result.Count());
            ClassicAssert.IsTrue(result.Any(p => p.IdProdus == 1 && p.Nume == "Produs 1"));
            ClassicAssert.IsTrue(result.Any(p => p.IdProdus == 2 && p.Nume == "Produs 2"));
        }

        [Test]
        public async Task GetProdusByComandaId_WhenNoProductsExist_ShouldReturnEmptyList()
        {
            await SeedProductsOrdersAndSuborders();

            var result = await _service.GetProdusByComandaId(999);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetAllProductsFromSubcomenzi_WhenProductsExist_ShouldReturnDistinctProducts()
        {
            await SeedProductsOrdersAndSuborders();

            var result = await _service.GetAllProductsFromSubcomenzi();

            ClassicAssert.AreEqual(2, result.Count());
            ClassicAssert.AreEqual(1, result.Count(p => p.IdProdus == 1));
            ClassicAssert.AreEqual(1, result.Count(p => p.IdProdus == 2));
        }

        [Test]
        public async Task GetAllProductsFromSubcomenzi_WhenNoSubcomenziExist_ShouldReturnEmptyList()
        {
            _context.Products.Add(CreateProduct(1, "Produs fara subcomanda"));
            await _context.SaveChangesAsync();

            var result = await _service.GetAllProductsFromSubcomenzi();

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetProdusByUserId_WhenProductsExist_ShouldReturnProductsOrderedByUser()
        {
            await SeedProductsOrdersAndSuborders();

            var result = await _service.GetProdusByUserId(10);

            ClassicAssert.AreEqual(2, result.Count());
            ClassicAssert.IsTrue(result.Any(p => p.IdProdus == 1));
            ClassicAssert.IsTrue(result.Any(p => p.IdProdus == 2));
        }

        [Test]
        public async Task GetProdusByUserId_WhenUserHasNoProducts_ShouldReturnEmptyList()
        {
            await SeedProductsOrdersAndSuborders();

            var result = await _service.GetProdusByUserId(999);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task DeleteSubcomanda_WhenExists_ShouldRemoveSubcomanda()
        {
            _context.Subcomenzi.Add(CreateSubcomanda(50, 1, 1, 1));
            await _context.SaveChangesAsync();

            await _service.DeleteSubcomanda(50);

            var deleted = await _context.Subcomenzi.FindAsync(50);

            ClassicAssert.IsNull(deleted);
        }

        [Test]
        public async Task DeleteSubcomanda_WhenMissing_ShouldNotThrow()
        {
            await _service.DeleteSubcomanda(999);

            ClassicAssert.AreEqual(0, _context.Subcomenzi.Count());
        }

        [Test]
        public async Task DeleteSubcomandaByComandaAndProdus_WhenExists_ShouldRemoveMatchingSubcomanda()
        {
            _context.Subcomenzi.AddRange(
                CreateSubcomanda(1, idProdus: 10, idComanda: 20, totalSubproduse: 1),
                CreateSubcomanda(2, idProdus: 11, idComanda: 20, totalSubproduse: 1)
            );

            await _context.SaveChangesAsync();

            await _service.DeleteSubcomandaByComandaAndProdus(20, 10);

            ClassicAssert.IsFalse(_context.Subcomenzi.Any(sc => sc.IdSubcomanda == 1));
            ClassicAssert.IsTrue(_context.Subcomenzi.Any(sc => sc.IdSubcomanda == 2));
        }

        [Test]
        public async Task DeleteSubcomandaByComandaAndProdus_WhenMissing_ShouldNotThrow()
        {
            _context.Subcomenzi.Add(CreateSubcomanda(1, idProdus: 10, idComanda: 20, totalSubproduse: 1));
            await _context.SaveChangesAsync();

            await _service.DeleteSubcomandaByComandaAndProdus(999, 999);

            ClassicAssert.AreEqual(1, _context.Subcomenzi.Count());
        }

        [Test]
        public async Task GetProdusByComandaIdResponse_WhenProductsExist_ShouldReturnOk()
        {
            await SeedProductsOrdersAndSuborders();

            var result = await _service.GetProdusByComandaIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var produse = ok.Value as IEnumerable<Produs>;

            ClassicAssert.IsNotNull(produse);
            ClassicAssert.AreEqual(2, produse!.Count());
        }

        [Test]
        public async Task GetProdusByComandaIdResponse_WhenNoProductsExist_ShouldReturnNotFound()
        {
            var result = await _service.GetProdusByComandaIdResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("No products found for this order.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task GetAllSubcomenziResponse_WhenSubcomenziExist_ShouldReturnOk()
        {
            await SeedProductsOrdersAndSuborders();

            var result = await _service.GetAllSubcomenziResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var subcomenzi = ok.Value as IEnumerable<Subcomanda>;

            ClassicAssert.IsNotNull(subcomenzi);
            ClassicAssert.AreEqual(3, subcomenzi!.Count());
        }

        [Test]
        public async Task GetAllSubcomenziResponse_WhenNoSubcomenziExist_ShouldReturnOkWithEmptyList()
        {
            var result = await _service.GetAllSubcomenziResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var subcomenzi = ok.Value as IEnumerable<Subcomanda>;

            ClassicAssert.IsNotNull(subcomenzi);
            ClassicAssert.AreEqual(0, subcomenzi!.Count());
        }

        [Test]
        public async Task GetSubcomandaByIdResponse_WhenSubcomandaExists_ShouldReturnOk()
        {
            _context.Subcomenzi.Add(CreateSubcomanda(1, 1, 1, 2));
            await _context.SaveChangesAsync();

            var result = await _service.GetSubcomandaByIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.IsInstanceOf<Subcomanda>(ok.Value);

            var subcomanda = ok.Value as Subcomanda;

            ClassicAssert.IsNotNull(subcomanda);
            ClassicAssert.AreEqual(1, subcomanda!.IdSubcomanda);
            ClassicAssert.AreEqual(2, subcomanda.TotalSubproduse);
        }

        [Test]
        public async Task GetSubcomandaByIdResponse_WhenSubcomandaMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetSubcomandaByIdResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Suborder not found.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task GetAllProductsFromSubcomenziResponse_WhenProductsExist_ShouldReturnOk()
        {
            await SeedProductsOrdersAndSuborders();

            var result = await _service.GetAllProductsFromSubcomenziResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var produse = ok.Value as IEnumerable<Produs>;

            ClassicAssert.IsNotNull(produse);
            ClassicAssert.AreEqual(2, produse!.Count());
        }

        [Test]
        public async Task GetAllProductsFromSubcomenziResponse_WhenNoProductsExist_ShouldReturnNotFound()
        {
            var result = await _service.GetAllProductsFromSubcomenziResponse();

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("No products found in any suborder.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task GetProdusByUserIdResponse_WhenProductsExist_ShouldReturnOk()
        {
            await SeedProductsOrdersAndSuborders();

            var result = await _service.GetProdusByUserIdResponse(10);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var produse = ok.Value as IEnumerable<Produs>;

            ClassicAssert.IsNotNull(produse);
            ClassicAssert.AreEqual(2, produse!.Count());
        }

        [Test]
        public async Task GetProdusByUserIdResponse_WhenNoProductsExist_ShouldReturnNotFound()
        {
            var result = await _service.GetProdusByUserIdResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("No products found for this user.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task CreateSubcomandaResponse_WhenSubcomandaIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.CreateSubcomandaResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Suborder data is required.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task CreateSubcomandaResponse_WhenSubcomandaIsValid_ShouldReturnCreatedAtActionAndPersist()
        {
            var subcomanda = new Subcomanda
            {
                IdProdus = 1,
                IdComanda = 1,
                TotalSubproduse = 5
            };

            var result = await _service.CreateSubcomandaResponse(subcomanda);

            var created = result as CreatedAtActionResult;

            ClassicAssert.IsNotNull(created);
            ClassicAssert.AreEqual(201, created!.StatusCode);
            ClassicAssert.AreEqual("GetSubcomandaById", created.ActionName);
            ClassicAssert.AreEqual("Subcomanda", created.ControllerName);
            ClassicAssert.IsInstanceOf<Subcomanda>(created.Value);

            var value = created.Value as Subcomanda;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.IsTrue(value!.IdSubcomanda > 0);
            ClassicAssert.AreEqual(5, value.TotalSubproduse);
            ClassicAssert.AreEqual(1, _context.Subcomenzi.Count());
        }

        [Test]
        public async Task DeleteSubcomandaResponse_WhenSubcomandaExists_ShouldReturnNoContentAndDelete()
        {
            _context.Subcomenzi.Add(CreateSubcomanda(1, 1, 1, 1));
            await _context.SaveChangesAsync();

            var result = await _service.DeleteSubcomandaResponse(1);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
            ClassicAssert.AreEqual(0, _context.Subcomenzi.Count());
        }

        [Test]
        public async Task DeleteSubcomandaResponse_WhenSubcomandaMissing_ShouldReturnNoContent()
        {
            var result = await _service.DeleteSubcomandaResponse(999);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
        }

        [Test]
        public async Task DeleteSubcomandaByComandaAndProdusResponse_WhenSubcomandaExists_ShouldReturnNoContentAndDelete()
        {
            _context.Subcomenzi.AddRange(
                CreateSubcomanda(1, idProdus: 10, idComanda: 20, totalSubproduse: 1),
                CreateSubcomanda(2, idProdus: 11, idComanda: 20, totalSubproduse: 1)
            );

            await _context.SaveChangesAsync();

            var result = await _service.DeleteSubcomandaByComandaAndProdusResponse(20, 10);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
            ClassicAssert.IsFalse(_context.Subcomenzi.Any(sc => sc.IdSubcomanda == 1));
            ClassicAssert.IsTrue(_context.Subcomenzi.Any(sc => sc.IdSubcomanda == 2));
        }

        [Test]
        public async Task DeleteSubcomandaByComandaAndProdusResponse_WhenSubcomandaMissing_ShouldReturnNoContent()
        {
            _context.Subcomenzi.Add(CreateSubcomanda(1, idProdus: 10, idComanda: 20, totalSubproduse: 1));
            await _context.SaveChangesAsync();

            var result = await _service.DeleteSubcomandaByComandaAndProdusResponse(999, 999);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
            ClassicAssert.AreEqual(1, _context.Subcomenzi.Count());
        }

        [Test]
        public async Task DeleteSubcomandaByComandaAndProdusResponse_WhenUnexpectedErrorOccurs_ShouldReturnNotFound()
        {
            _context.Dispose();

            var result = await _service.DeleteSubcomandaByComandaAndProdusResponse(1, 1);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual(
                "Suborder not found for the given order and product.",
                GetAnonymousProperty(notFound.Value!, "message")
            );
        }
    }
}