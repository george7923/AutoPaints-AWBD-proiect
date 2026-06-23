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
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.UnitTests
{
    [TestFixture]
    public class ComandaServiceTests
    {
        private ApplicationDbContext _context = null!;
        private Mock<ICosRepository> _cosRepositoryMock = null!;
        private Mock<ISubcomandaRepository> _subcomandaRepositoryMock = null!;
        private ComandaService _service = null!;

        [SetUp]
        public void Before()
        {
            _context = TestDbContextFactory.CreateContext();

            _cosRepositoryMock = new Mock<ICosRepository>();
            _subcomandaRepositoryMock = new Mock<ISubcomandaRepository>();

            _service = new ComandaService(
                _context,
                _cosRepositoryMock.Object,
                _subcomandaRepositoryMock.Object
            );
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

        private async Task SeedUser(int userId = 1, int persoanaId = 1)
        {
            _context.Persoane.Add(new Persoana
            {
                IdPersoana = persoanaId,
                Nume = "User",
                Prenume = "Test",
                Email = $"user{userId}@test.com",
                tipPersoana = "Fizica",
                Telefon = "0711111111",
                Rol = "Participant"
            });

            _context.Users.Add(new User
            {
                IdUser = userId,
                Username = $"user{userId}",
                Password = "pass",
                IdPersoana = persoanaId
            });

            await _context.SaveChangesAsync();
        }

        private async Task SeedAddressGraph(int idAdresa = 1)
        {
            _context.Tari.Add(new Tari
            {
                IdTara = 1,
                DenumireTara = "Romania"
            });

            _context.Judete.Add(new Judete
            {
                IdJudet = 1,
                DenumireJudet = "Constanta",
                IdTara = 1
            });

            _context.Localitati.Add(new Localitati
            {
                IdLocalitate = 1,
                DenumireLocalitate = "Constanta",
                IdJudet = 1
            });

            _context.Strazi.Add(new Strazi
            {
                IdStrada = 1,
                DenumireStrada = "Tomis",
                Nr = 10,
                IdLocalitate = 1
            });

            _context.Adrese.Add(new Adresa
            {
                IdAdresa = idAdresa,
                IdStrada = 1,
                Bloc = "A",
                Scara = "1",
                Etaj = "2",
                Apartament = "3"
            });

            await _context.SaveChangesAsync();
        }

        private async Task SeedCartWithProducts(int userId = 1, int idCos = 10)
        {
            _context.Cosuri.Add(new Cos
            {
                idCos = idCos,
                CodUnic = "COS-TEST",
                IdUser = userId,
                DataCreare = DateTime.UtcNow
            });

            _context.Products.AddRange(
                new Produs
                {
                    IdProdus = 1,
                    Nume = "Produs 1",
                    Descriere = "Descriere 1",
                    EsteSpray = false,
                    Valabil = true,
                    IdCategorie = 1
                },
                new Produs
                {
                    IdProdus = 2,
                    Nume = "Produs 2",
                    Descriere = "Descriere 2",
                    EsteSpray = true,
                    Valabil = true,
                    IdCategorie = 1
                }
            );

            _context.SubProduse.AddRange(
                new Subprodus
                {
                    IdSubprodus = 1,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = idCos
                },
                new Subprodus
                {
                    IdSubprodus = 2,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = idCos
                },
                new Subprodus
                {
                    IdSubprodus = 3,
                    IdProdus = 2,
                    Valabil = true,
                    idCos = idCos
                }
            );

            _context.Preturi_Produs.AddRange(
                new Preturi_Produs
                {
                    idPP = 1,
                    IdProdus = 1,
                    Pret = 25,
                    DataInceput = DateTime.UtcNow.AddDays(-2)
                },
                new Preturi_Produs
                {
                    idPP = 2,
                    IdProdus = 2,
                    Pret = 40,
                    DataInceput = DateTime.UtcNow.AddDays(-1)
                }
            );

            await _context.SaveChangesAsync();
        }

        private async Task SeedOrderWithDetails(int userId = 7, int idComanda = 1)
        {
            await SeedUser(userId, userId);
            await SeedAddressGraph(1);

            _context.Products.Add(new Produs
            {
                IdProdus = 1,
                Nume = "Produs",
                Descriere = "D",
                EsteSpray = false,
                Valabil = true,
                IdCategorie = 1
            });

            _context.Preturi_Produs.Add(new Preturi_Produs
            {
                idPP = 1,
                IdProdus = 1,
                Pret = 50,
                DataInceput = DateTime.UtcNow
            });

            _context.Comenzi.Add(new Comanda
            {
                IdComanda = idComanda,
                IdUser = userId,
                IdAdresa = 1,
                ETA = DateTime.UtcNow,
                IsPlaced = false,
                PretTotal = 100,
                IdCard_CC = 0
            });

            _context.Subcomenzi.Add(new Subcomanda
            {
                IdSubcomanda = 1,
                IdComanda = idComanda,
                IdProdus = 1,
                TotalSubproduse = 2
            });

            await _context.SaveChangesAsync();
        }

        private static Comanda CreateComandaEntity(int id = 1, double pretTotal = 100, bool isPlaced = false)
        {
            return new Comanda
            {
                IdComanda = id,
                IdUser = 1,
                IdAdresa = 1,
                ETA = DateTime.UtcNow,
                IsPlaced = isPlaced,
                PretTotal = pretTotal
            };
        }

        private static ComandaEmitereDTO CreateEmitereDto(int userId = 1, int idAdresa = 1, int? idCard = null)
        {
            return new ComandaEmitereDTO
            {
                IdUser = userId,
                IdAdresa = idAdresa,
                IdCard = idCard
            };
        }

        [Test]
        public async Task GetById_ShouldReturnNullBecauseMethodIsStub()
        {
            var result = await _service.GetById(1);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task GetAllOrders_ShouldReturnNullBecauseMethodIsStub()
        {
            var result = await _service.GetAllOrders();

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task GetComenziByUserId_ShouldReturnNullBecauseMethodIsStub()
        {
            var result = await _service.GetComenziByUserId(1);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task SubmitComanda_ShouldReturnNullBecauseMethodIsStub()
        {
            var result = await _service.SubmitComanda(new ComandaSubmitDTO
            {
                IdUser = 1,
                IdAdresa = 1,
                PretTotal = 100
            });

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task CreateComanda_ShouldPersistOrder()
        {
            await _service.CreateComanda(new Comanda
            {
                IdUser = 1,
                IdAdresa = 1,
                ETA = DateTime.UtcNow,
                IsPlaced = false,
                PretTotal = 100
            });

            ClassicAssert.AreEqual(1, _context.Comenzi.Count(c => c.PretTotal == 100));
        }

        [Test]
        public async Task UpdateComanda_ShouldModifyOrder()
        {
            _context.Comenzi.Add(CreateComandaEntity(1, 100, false));
            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            await _service.UpdateComanda(new Comanda
            {
                IdComanda = 1,
                IdUser = 1,
                IdAdresa = 1,
                ETA = DateTime.UtcNow,
                IsPlaced = true,
                PretTotal = 150
            });

            var updated = await _context.Comenzi.FindAsync(1);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual(150, updated!.PretTotal);
            ClassicAssert.IsTrue(updated.IsPlaced);
        }

        [Test]
        public async Task DeleteComanda_WhenExists_ShouldRemoveOrder()
        {
            _context.Comenzi.Add(CreateComandaEntity(2, 100, false));
            await _context.SaveChangesAsync();

            await _service.DeleteComanda(2);

            var deleted = await _context.Comenzi.FindAsync(2);

            ClassicAssert.IsNull(deleted);
        }

        [Test]
        public async Task DeleteComanda_WhenMissing_ShouldNotThrow()
        {
            await _service.DeleteComanda(999);

            ClassicAssert.AreEqual(0, _context.Comenzi.Count());
        }

        [Test]
        public async Task MarcheazaComandaCaLivrata_WhenOrderExists_ShouldReturnTrueAndSetPlaced()
        {
            _context.Comenzi.Add(CreateComandaEntity(3, 100, false));
            await _context.SaveChangesAsync();

            var result = await _service.MarcheazaComandaCaLivrata(3);

            var updated = await _context.Comenzi.FindAsync(3);

            ClassicAssert.IsTrue(result);
            ClassicAssert.IsNotNull(updated);
            ClassicAssert.IsTrue(updated!.IsPlaced);
        }

        [Test]
        public async Task MarcheazaComandaCaLivrata_WhenOrderMissing_ShouldReturnFalse()
        {
            var result = await _service.MarcheazaComandaCaLivrata(999);

            ClassicAssert.IsFalse(result);
        }

        [Test]
        public void EmitereComanda_WhenCartMissing_ShouldThrowException()
        {
            var ex = Assert.ThrowsAsync<Exception>(async () =>
            {
                await _service.EmitereComanda(1, 1, null);
            });

            ClassicAssert.IsNotNull(ex);
            ClassicAssert.IsTrue(
                ex!.Message.Contains("Coșul nu a fost găsit") ||
                ex.Message.Contains("Cosul nu a fost gasit")
            );
        }

        [Test]
        public async Task EmitereComanda_WhenCartEmpty_ShouldThrowException()
        {
            _context.Cosuri.Add(new Cos
            {
                idCos = 10,
                CodUnic = "C",
                IdUser = 1,
                DataCreare = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var ex = Assert.ThrowsAsync<Exception>(async () =>
            {
                await _service.EmitereComanda(1, 1, null);
            });

            ClassicAssert.IsNotNull(ex);
            ClassicAssert.IsTrue(
                ex!.Message.Contains("Coșul este gol") ||
                ex.Message.Contains("Cosul este gol")
            );
        }

        [Test]
        public async Task EmitereComanda_WhenCartHasSubproducts_ShouldCreateOrderAndSubcommands()
        {
            await SeedCartWithProducts(1, 10);

            var idComanda = await _service.EmitereComanda(1, 1, null);

            ClassicAssert.IsTrue(idComanda > 0);
            ClassicAssert.AreEqual(1, _context.Comenzi.Count(c => c.PretTotal == 90));
            ClassicAssert.AreEqual(2, _context.Subcomenzi.Count(sc => sc.IdComanda == idComanda));
            ClassicAssert.AreEqual(
                1,
                _context.Subcomenzi.Count(sc => sc.IdComanda == idComanda && sc.IdProdus == 1 && sc.TotalSubproduse == 2)
            );
            ClassicAssert.AreEqual(
                1,
                _context.Subcomenzi.Count(sc => sc.IdComanda == idComanda && sc.IdProdus == 2 && sc.TotalSubproduse == 1)
            );
            ClassicAssert.IsTrue(_context.SubProduse.All(sp => sp.idCos == null));
        }

        [Test]
        public async Task EmitereComanda_WhenCardIdIsProvided_ShouldCreateOrderWithCard()
        {
            await SeedCartWithProducts(1, 10);

            var idComanda = await _service.EmitereComanda(1, 1, 5);

            var comanda = await _context.Comenzi.FindAsync(idComanda);

            ClassicAssert.IsNotNull(comanda);
            ClassicAssert.AreEqual(5, comanda!.IdCard_CC);
        }

        [Test]
        public async Task GetToateComenzileAleUtilizatorului_ShouldReturnOrdersWithDetails()
        {
            await SeedOrderWithDetails(7, 1);

            var result = await _service.GetToateComenzileAleUtilizatorului(7);

            ClassicAssert.AreEqual(1, result.Count(c => c.IdComanda == 1));

            var order = result.First(c => c.IdComanda == 1);

            ClassicAssert.AreEqual("Romania", order.Tara);
            ClassicAssert.AreEqual("Constanta", order.Judet);
            ClassicAssert.AreEqual("Constanta", order.Localitate);
            ClassicAssert.AreEqual("Tomis", order.Strada);
            ClassicAssert.AreEqual(1, order.Produse.Count(p => p.NumeProdus == "Produs"));
        }

        [Test]
        public async Task GetToateComenzileAleUtilizatorului_WhenUserHasNoOrders_ShouldReturnEmptyList()
        {
            var result = await _service.GetToateComenzileAleUtilizatorului(999);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetComandaByIdResponse_WhenComandaDoesNotExist_ShouldReturnNotFound()
        {
            var result = await _service.GetComandaByIdResponse(999);

            ClassicAssert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task GetAllComenziResponse_ShouldReturnOkWithNullBecauseGetAllOrdersIsStub()
        {
            var result = await _service.GetAllComenziResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.IsNull(ok.Value);
        }

        [Test]
        public async Task GetComenziByUserIdResponse_WhenNoOrdersBecauseMethodIsStub_ShouldReturnNotFound()
        {
            var result = await _service.GetComenziByUserIdResponse(1);

            ClassicAssert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task SubmitComandaWithPaymentResponse_WhenDtoIsNull_ShouldReturnBadRequest_AndNotCallRepositories()
        {
            var result = await _service.SubmitComandaWithPaymentResponse(null!);

            ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);

            _cosRepositoryMock.Verify(x => x.GetProductsByUserId(It.IsAny<int>()), Times.Never);
            _cosRepositoryMock.Verify(x => x.ClearCart(It.IsAny<int>()), Times.Never);
            _subcomandaRepositoryMock.Verify(x => x.AddSubcomanda(It.IsAny<Subcomanda>()), Times.Never);
        }

        [Test]
        public async Task SubmitComandaWithPaymentResponse_WhenPaymentIsNull_ShouldReturnBadRequest_AndNotCallRepositories()
        {
            var dto = new ComandaPaymentDTO
            {
                Payment = null!,
                Comanda = new ComandaSubmitDTO
                {
                    IdUser = 1,
                    IdAdresa = 1,
                    PretTotal = 100
                }
            };

            var result = await _service.SubmitComandaWithPaymentResponse(dto);

            ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);

            _cosRepositoryMock.Verify(x => x.GetProductsByUserId(It.IsAny<int>()), Times.Never);
            _cosRepositoryMock.Verify(x => x.ClearCart(It.IsAny<int>()), Times.Never);
            _subcomandaRepositoryMock.Verify(x => x.AddSubcomanda(It.IsAny<Subcomanda>()), Times.Never);
        }

        [Test]
        public async Task SubmitComandaWithPaymentResponse_WhenComandaIsNull_ShouldReturnBadRequest_AndNotCallRepositories()
        {
            var dto = new ComandaPaymentDTO
            {
                Payment = new PaymentDTO
                {
                    AmountInBani = 10000,
                    PaymentMethodId = "pm_test",
                    Description = "Test payment"
                },
                Comanda = null!
            };

            var result = await _service.SubmitComandaWithPaymentResponse(dto);

            ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);

            _cosRepositoryMock.Verify(x => x.GetProductsByUserId(It.IsAny<int>()), Times.Never);
            _cosRepositoryMock.Verify(x => x.ClearCart(It.IsAny<int>()), Times.Never);
            _subcomandaRepositoryMock.Verify(x => x.AddSubcomanda(It.IsAny<Subcomanda>()), Times.Never);
        }

        [Test]
        public async Task UpdateComandaResponse_WhenIdDoesNotMatch_ShouldReturnBadRequest()
        {
            var comanda = CreateComandaEntity(2, 100, false);

            var result = await _service.UpdateComandaResponse(1, comanda);

            ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task UpdateComandaResponse_WhenComandaDoesNotExistBecauseGetByIdIsStub_ShouldReturnNotFound()
        {
            var comanda = CreateComandaEntity(1, 100, false);

            var result = await _service.UpdateComandaResponse(1, comanda);

            ClassicAssert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task DeleteComandaResponse_WhenComandaDoesNotExist_ShouldReturnNotFound()
        {
            var result = await _service.DeleteComandaResponse(999);

            ClassicAssert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task EmitereComandaResponse_WhenModelIsInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.EmitereComandaResponse(CreateEmitereDto(), false);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Datele trimise nu sunt valide.", badRequest.Value);
        }

        [Test]
        public async Task EmitereComandaResponse_WhenValid_ShouldReturnOkAndMarkSubproductsUnavailable()
        {
            await SeedCartWithProducts(1, 10);

            var result = await _service.EmitereComandaResponse(CreateEmitereDto(1, 1, null), true);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual(
                "Comanda a fost înregistrată, subprodusele setate ca nevalabile.",
                GetAnonymousProperty(ok.Value!, "message")
            );

            ClassicAssert.AreEqual(1, _context.Comenzi.Count());
            ClassicAssert.IsTrue(_context.SubProduse.All(sp => sp.Valabil == false));
        }

        [Test]
        public async Task EmitereComandaResponse_WhenServiceThrows_ShouldReturnInternalServerError()
        {
            var result = await _service.EmitereComandaResponse(CreateEmitereDto(999, 1, null), true);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("Eroare la emiterea comenzii", GetAnonymousProperty(objectResult.Value!, "error"));
        }

        [Test]
        public async Task MarcheazaCaLivrataSauNelivrataResponse_WhenComandaMissing_ShouldReturnNotFound()
        {
            var result = await _service.MarcheazaCaLivrataSauNelivrataResponse(999, new LivrareDto
            {
                Livrata = true
            });

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Comanda nu a fost găsită.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task MarcheazaCaLivrataSauNelivrataResponse_WhenComandaExists_ShouldReturnOkAndUpdateLivrare()
        {
            _context.Comenzi.Add(CreateComandaEntity(1, 100, false));
            await _context.SaveChangesAsync();

            var result = await _service.MarcheazaCaLivrataSauNelivrataResponse(1, new LivrareDto
            {
                Livrata = true
            });

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual(
                "Comanda a fost marcată ca livrată: True",
                GetAnonymousProperty(ok.Value!, "message")
            );

            var updated = await _context.Comenzi.FindAsync(1);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.IsTrue(updated!.IsPlaced);
        }

        [Test]
        public async Task MarcheazaCaLivrataSauNelivrataResponse_WhenUnexpectedErrorOccurs_ShouldReturnInternalServerError()
        {
            _context.Dispose();

            var result = await _service.MarcheazaCaLivrataSauNelivrataResponse(1, new LivrareDto
            {
                Livrata = true
            });

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("Eroare la modificarea livrării.", GetAnonymousProperty(objectResult.Value!, "error"));
        }

        [Test]
        public async Task GetToateComenzileAleUtilizatoruluiResponse_ShouldReturnOkWithOrders()
        {
            await SeedOrderWithDetails(7, 1);

            var result = await _service.GetToateComenzileAleUtilizatoruluiResponse(7);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var orders = ok.Value as IEnumerable<ComandaCuDetaliiDTO>;

            ClassicAssert.IsNotNull(orders);
            ClassicAssert.AreEqual(1, orders!.Count());
        }

        [Test]
        public async Task EmitereComandaCashResponse_WhenModelIsInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.EmitereComandaCashResponse(CreateEmitereDto(), false);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Datele trimise nu sunt valide.", badRequest.Value);
        }

        [Test]
        public async Task EmitereComandaCashResponse_WhenValid_ShouldReturnOkAndCreateCashOrder()
        {
            await SeedCartWithProducts(1, 10);

            var result = await _service.EmitereComandaCashResponse(CreateEmitereDto(1, 1, null), true);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual(
                "Comanda (cash) a fost înregistrată și subprodusele au fost marcate nevalabile.",
                GetAnonymousProperty(ok.Value!, "message")
            );

            var comanda = _context.Comenzi.Single();

            ClassicAssert.IsNull(comanda.IdCard_CC);
            ClassicAssert.IsTrue(_context.SubProduse.All(sp => sp.Valabil == false));
        }

        [Test]
        public async Task EmitereComandaCashResponse_WhenServiceThrows_ShouldReturnInternalServerError()
        {
            var result = await _service.EmitereComandaCashResponse(CreateEmitereDto(999, 1, null), true);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual(
                "Eroare la emiterea comenzii cu plată la domiciliu",
                GetAnonymousProperty(objectResult.Value!, "error")
            );
        }

        [Test]
        public async Task GetComenziScurtByUserResponse_ShouldReturnOkWithShortOrders()
        {
            _context.Comenzi.AddRange(
                new Comanda
                {
                    IdComanda = 1,
                    IdUser = 5,
                    IdAdresa = 1,
                    ETA = DateTime.UtcNow,
                    IsPlaced = false,
                    PretTotal = 100,
                    IdCard_CC = null
                },
                new Comanda
                {
                    IdComanda = 2,
                    IdUser = 5,
                    IdAdresa = 1,
                    ETA = DateTime.UtcNow,
                    IsPlaced = true,
                    PretTotal = 200,
                    IdCard_CC = 3
                },
                new Comanda
                {
                    IdComanda = 3,
                    IdUser = 9,
                    IdAdresa = 1,
                    ETA = DateTime.UtcNow,
                    IsPlaced = true,
                    PretTotal = 300,
                    IdCard_CC = null
                }
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetComenziScurtByUserResponse(5);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var items = AnonymousList(ok.Value!);

            ClassicAssert.AreEqual(2, items.Count);
            ClassicAssert.AreEqual("Plata la domiciliu", GetAnonymousProperty(items[0], "Plata"));
            ClassicAssert.AreEqual("Plata cu cardul", GetAnonymousProperty(items[1], "Plata"));
        }

        [Test]
        public async Task GetComenziScurtByUserResponse_WhenUserHasNoOrders_ShouldReturnOkWithEmptyList()
        {
            var result = await _service.GetComenziScurtByUserResponse(999);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var items = AnonymousList(ok.Value!);

            ClassicAssert.AreEqual(0, items.Count);
        }

        [Test]
        public async Task GenerareBonFiscalResponse_WhenComandaMissing_ShouldReturnNotFound()
        {
            var result = await _service.GenerareBonFiscalResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Comanda nu a fost găsită", notFound.Value);
        }

        [Test]
        public async Task GenerareBonFiscalResponse_WhenComandaExists_ShouldReturnPdfFile()
        {
            await SeedOrderWithDetails(7, 1);

            var result = await _service.GenerareBonFiscalResponse(1);

            var file = result as FileStreamResult;

            ClassicAssert.IsNotNull(file);
            ClassicAssert.AreEqual("application/pdf", file!.ContentType);
            ClassicAssert.AreEqual("bon-fiscal.pdf", file.FileDownloadName);
            ClassicAssert.IsNotNull(file.FileStream);
            ClassicAssert.IsTrue(file.FileStream.Length > 0);
        }

        [Test]
        public async Task GenerareBonFiscalResponse_WhenUnexpectedErrorOccurs_ShouldReturnInternalServerError()
        {
            _context.Dispose();

            var result = await _service.GenerareBonFiscalResponse(1);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.IsTrue(objectResult.Value!.ToString()!.Contains("Eroare internă la generarea PDF-ului"));
        }
    }
}