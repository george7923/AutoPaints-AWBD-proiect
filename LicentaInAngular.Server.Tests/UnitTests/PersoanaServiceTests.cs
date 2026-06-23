using System.Linq;
using System.Threading.Tasks;
using LicentaInAngular.Server.Controllers;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.UnitTests
{
    [TestFixture]
    public class PersoanaServiceTests
    {
        private ApplicationDbContext _context = null!;
        private PersoanaService _service = null!;

        [SetUp]
        public void Before()
        {
            _context = TestDbContextFactory.CreateContext();
            _service = new PersoanaService(_context);
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

        private static Persoana CreatePersoana(
            int id = 1,
            string nume = "Popescu",
            string prenume = "Ion",
            string email = "ion@test.com",
            string tipPersoana = "Fizica",
            string telefon = "0711111111",
            string rol = "Participant")
        {
            return new Persoana
            {
                IdPersoana = id,
                Nume = nume,
                Prenume = prenume,
                Email = email,
                tipPersoana = tipPersoana,
                Telefon = telefon,
                Rol = rol
            };
        }

        [Test]
        public async Task CreatePersoana_ShouldPersistPersoana()
        {
            await _service.CreatePersoana(new Persoana
            {
                Nume = "Popescu",
                Prenume = "Ion",
                Email = "ion@test.com",
                tipPersoana = "Fizica",
                Telefon = "0711111111",
                Rol = "Participant"
            });

            ClassicAssert.AreEqual(1, _context.Persoane.Count(p => p.Email == "ion@test.com"));
        }

        [Test]
        public async Task GetAll_ShouldReturnAllPeople()
        {
            _context.Persoane.AddRange(
                CreatePersoana(1, "A", "B", "a@test.com", "Fizica", "1", "Participant"),
                CreatePersoana(2, "C", "D", "c@test.com", "Fizica", "2", "Administrator")
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetAll();

            ClassicAssert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetAll_WhenEmpty_ShouldReturnEmptyList()
        {
            var result = await _service.GetAll();

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetById_WhenExists_ShouldReturnPersoana()
        {
            _context.Persoane.Add(CreatePersoana(5, "Ionescu", "Ana", "ana@test.com"));
            await _context.SaveChangesAsync();

            var result = await _service.GetById(5);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("ana@test.com", result!.Email);
            ClassicAssert.AreEqual("Ionescu", result.Nume);
            ClassicAssert.AreEqual("Ana", result.Prenume);
        }

        [Test]
        public async Task GetById_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetById(999);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task GetByEmail_WhenExists_ShouldIgnoreCase()
        {
            _context.Persoane.Add(CreatePersoana(1, "Test", "User", "person@test.com"));
            await _context.SaveChangesAsync();

            var result = await _service.GetByEmail("PERSON@test.com");

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("Test", result!.Nume);
            ClassicAssert.AreEqual("person@test.com", result.Email);
        }

        [Test]
        public async Task GetByEmail_WhenMissing_ShouldReturnNull()
        {
            _context.Persoane.Add(CreatePersoana(1, "Test", "User", "person@test.com"));
            await _context.SaveChangesAsync();

            var result = await _service.GetByEmail("missing@test.com");

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task UpdatePersoanaById_WhenExists_ShouldModifyExistingValues()
        {
            _context.Persoane.Add(CreatePersoana(
                10,
                "Old",
                "OldPrenume",
                "old@test.com",
                "Fizica",
                "1",
                "Participant"
            ));

            await _context.SaveChangesAsync();

            await _service.UpdatePersoanaById(10, new Persoana
            {
                Nume = "New",
                Prenume = "NewPrenume",
                Email = "new@test.com",
                tipPersoana = "Juridica",
                Telefon = "2"
            });

            var updated = await _context.Persoane.FindAsync(10);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("New", updated!.Nume);
            ClassicAssert.AreEqual("NewPrenume", updated.Prenume);
            ClassicAssert.AreEqual("new@test.com", updated.Email);
            ClassicAssert.AreEqual("Juridica", updated.tipPersoana);
            ClassicAssert.AreEqual("2", updated.Telefon);
        }

        [Test]
        public async Task UpdatePersoanaById_WhenSomeFieldsAreNull_ShouldKeepOldValues()
        {
            _context.Persoane.Add(CreatePersoana(
                10,
                "Old",
                "OldPrenume",
                "old@test.com",
                "Fizica",
                "1",
                "Participant"
            ));

            await _context.SaveChangesAsync();

            await _service.UpdatePersoanaById(10, new Persoana
            {
                Nume = "New",
                Prenume = null,
                Email = null,
                tipPersoana = null,
                Telefon = "2"
            });

            var updated = await _context.Persoane.FindAsync(10);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("New", updated!.Nume);
            ClassicAssert.AreEqual("OldPrenume", updated.Prenume);
            ClassicAssert.AreEqual("old@test.com", updated.Email);
            ClassicAssert.AreEqual("Fizica", updated.tipPersoana);
            ClassicAssert.AreEqual("2", updated.Telefon);
        }

        [Test]
        public async Task UpdatePersoanaById_WhenPersoanaMissing_ShouldNotThrowAndNotCreateAnything()
        {
            await _service.UpdatePersoanaById(999, new Persoana
            {
                Nume = "New",
                Email = "new@test.com"
            });

            ClassicAssert.AreEqual(0, _context.Persoane.Count());
        }

        [Test]
        public async Task DeletePersoanaById_WhenExists_ShouldRemovePersoana()
        {
            _context.Persoane.Add(CreatePersoana(99, "Delete", "Me", "del@test.com"));
            await _context.SaveChangesAsync();

            await _service.DeletePersoanaById(99);

            var deleted = await _context.Persoane.FindAsync(99);

            ClassicAssert.IsNull(deleted);
        }

        [Test]
        public async Task DeletePersoanaById_WhenMissing_ShouldNotThrow()
        {
            await _service.DeletePersoanaById(999);

            ClassicAssert.AreEqual(0, _context.Persoane.Count());
        }

        [Test]
        public async Task GetByIdResponse_WhenPersoanaExists_ShouldReturnOk()
        {
            _context.Persoane.Add(CreatePersoana(1, "Popescu", "Ion", "ion@test.com"));
            await _context.SaveChangesAsync();

            var result = await _service.GetByIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.IsInstanceOf<Persoana>(ok!.Value);

            var value = ok.Value as Persoana;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.AreEqual(1, value!.IdPersoana);
            ClassicAssert.AreEqual("ion@test.com", value.Email);
        }

        [Test]
        public async Task GetByIdResponse_WhenPersoanaMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetByIdResponse(999);

            ClassicAssert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task GetByEmailResponse_WhenPersoanaExists_ShouldReturnOk()
        {
            _context.Persoane.Add(CreatePersoana(1, "Popescu", "Ion", "ion@test.com"));
            await _context.SaveChangesAsync();

            var result = await _service.GetByEmailResponse("ION@test.com");

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.IsInstanceOf<Persoana>(ok!.Value);

            var value = ok.Value as Persoana;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.AreEqual("ion@test.com", value!.Email);
        }

        [Test]
        public async Task GetByEmailResponse_WhenPersoanaMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetByEmailResponse("missing@test.com");

            ClassicAssert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task CreateResponse_ShouldCreatePersoanaAndReturnCreatedAtAction()
        {
            var persoana = new Persoana
            {
                Nume = "Create",
                Prenume = "Test",
                Email = "create@test.com",
                tipPersoana = "Fizica",
                Telefon = "0711111111",
                Rol = "Participant"
            };

            var result = await _service.CreateResponse(persoana);

            var created = result as CreatedAtActionResult;

            ClassicAssert.IsNotNull(created);
            ClassicAssert.AreEqual("GetById", created!.ActionName);
            ClassicAssert.AreEqual("Persoana", created.ControllerName);
            ClassicAssert.IsInstanceOf<Persoana>(created.Value);

            var value = created.Value as Persoana;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.IsTrue(value!.IdPersoana > 0);
            ClassicAssert.AreEqual("create@test.com", value.Email);
            ClassicAssert.AreEqual(1, _context.Persoane.Count(p => p.Email == "create@test.com"));
        }

        [Test]
        public async Task UpdatePersoanaByIdResponse_WhenUpdatedPersoanaIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.UpdatePersoanaByIdResponse(1, null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Invalid Persoana data", GetAnonymousProperty(badRequest!.Value!, "error"));
        }

        [Test]
        public async Task UpdatePersoanaByIdResponse_WhenValid_ShouldReturnOkAndUpdatePersoana()
        {
            _context.Persoane.Add(CreatePersoana(1, "Old", "OldPrenume", "old@test.com"));
            await _context.SaveChangesAsync();

            var result = await _service.UpdatePersoanaByIdResponse(1, new Persoana
            {
                Nume = "New",
                Email = "new@test.com"
            });

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(
                "Persoana actualizata cu succes",
                GetAnonymousProperty(ok!.Value!, "message")
            );

            var updated = await _context.Persoane.FindAsync(1);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("New", updated!.Nume);
            ClassicAssert.AreEqual("new@test.com", updated.Email);
            ClassicAssert.AreEqual("OldPrenume", updated.Prenume);
        }

        [Test]
        public async Task UpdatePersoanaByIdResponse_WhenPersoanaMissing_ShouldStillReturnOk()
        {
            var result = await _service.UpdatePersoanaByIdResponse(999, new Persoana
            {
                Nume = "New"
            });

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(
                "Persoana actualizata cu succes",
                GetAnonymousProperty(ok!.Value!, "message")
            );
            ClassicAssert.AreEqual(0, _context.Persoane.Count());
        }

        [Test]
        public async Task DeletePersoanaByIdResponse_WhenPersoanaMissing_ShouldReturnNotFound()
        {
            var result = await _service.DeletePersoanaByIdResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Persoana not found", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task DeletePersoanaByIdResponse_WhenPersoanaExists_ShouldReturnOkAndDelete()
        {
            _context.Persoane.Add(CreatePersoana(1, "Delete", "Me", "delete@test.com"));
            await _context.SaveChangesAsync();

            var result = await _service.DeletePersoanaByIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(
                "Persoana deleted successfully",
                GetAnonymousProperty(ok!.Value!, "message")
            );

            ClassicAssert.AreEqual(0, _context.Persoane.Count());
        }

        [Test]
        public void SendEmailResponse_WhenEmailAddressIsInvalid_ShouldReturnInternalServerError()
        {
            var result = _service.SendEmailResponse(new PersoanaController.EmailRequest
            {
                To = "email-invalid",
                Subject = "Test",
                Body = "Body test"
            });

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("Eroare la trimiterea emailului", GetAnonymousProperty(objectResult.Value!, "error"));
            ClassicAssert.IsNotNull(GetAnonymousProperty(objectResult.Value!, "details"));
        }

        [Test]
        public void SendEmailResponse_WhenRequestIsNull_ShouldReturnInternalServerError()
        {
            var result = _service.SendEmailResponse(null!);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("Eroare la trimiterea emailului", GetAnonymousProperty(objectResult.Value!, "error"));
            ClassicAssert.IsNotNull(GetAnonymousProperty(objectResult.Value!, "details"));
        }
    }
}