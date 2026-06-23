using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;
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
    public class AdresaServiceTests
    {
        private ApplicationDbContext _context = null!;
        private AdresaService _service = null!;

        [SetUp]
        public void Before()
        {
            _context = TestDbContextFactory.CreateContext();
            _service = new AdresaService(_context);
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

        private static AdresaNestedDTO CreateValidAdresaDto()
        {
            return new AdresaNestedDTO
            {
                Bloc = "A",
                Scara = "1",
                Etaj = "2",
                Apartament = "3",
                Strazi = new StraziDTO
                {
                    DenumireStrada = "Tomis",
                    Nr = 10,
                    Localitati = new LocalitatiDTO
                    {
                        DenumireLocalitate = "Constanta",
                        Judete = new JudeteDTO
                        {
                            DenumireJudet = "Constanta",
                            Tari = new TariDTO
                            {
                                DenumireTara = "Romania"
                            }
                        }
                    }
                }
            };
        }

        private static UserAddressDTO CreateUserAddressDto()
        {
            return new UserAddressDTO
            {
                IdUser = 1,
                IdAdresa = 1
            };
        }

        private async Task SeedAddressGraph()
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
                IdAdresa = 1,
                IdStrada = 1,
                Bloc = "A",
                Scara = "1",
                Etaj = "2",
                Apartament = "3"
            });

            await _context.SaveChangesAsync();
        }

        private async Task SeedUser()
        {
            _context.Persoane.Add(new Persoana
            {
                IdPersoana = 1,
                Nume = "User",
                Prenume = "Test",
                Email = "u@test.com",
                tipPersoana = "Fizica",
                Telefon = "1",
                Rol = "Participant"
            });

            _context.Users.Add(new User
            {
                IdUser = 1,
                Username = "user",
                Password = "pass",
                IdPersoana = 1
            });

            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task GetById_WhenExists_ShouldReturnAddressWithNestedData()
        {
            await SeedAddressGraph();

            var result = await _service.GetById(1);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsNotNull(result!.Strazi);
            ClassicAssert.AreEqual("Tomis", result.Strazi!.DenumireStrada);
            ClassicAssert.IsNotNull(result.Strazi.Localitati);
            ClassicAssert.IsNotNull(result.Strazi.Localitati!.Judete);
            ClassicAssert.IsNotNull(result.Strazi.Localitati.Judete!.Tari);
            ClassicAssert.AreEqual("Romania", result.Strazi.Localitati.Judete.Tari!.DenumireTara);
        }

        [Test]
        public async Task GetById_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetById(999);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task CreateAdresaSiAsigneazaUser_WhenDataIsNew_ShouldCreateFullAddressGraphAndRelation()
        {
            await SeedUser();

            var dto = CreateValidAdresaDto();

            var result = await _service.CreateAdresaSiAsigneazaUser(dto, 1);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsTrue(result.IdAdresa > 0);
            ClassicAssert.AreEqual("A", result.Bloc);
            ClassicAssert.AreEqual("1", result.Scara);
            ClassicAssert.AreEqual("2", result.Etaj);
            ClassicAssert.AreEqual("3", result.Apartament);

            ClassicAssert.AreEqual(1, _context.Tari.Count());
            ClassicAssert.AreEqual(1, _context.Judete.Count());
            ClassicAssert.AreEqual(1, _context.Localitati.Count());
            ClassicAssert.AreEqual(1, _context.Strazi.Count());
            ClassicAssert.AreEqual(1, _context.Adrese.Count());

            var relation = _context.Adrese_Useri
                .SingleOrDefault(au => au.IdUser == 1 && au.IdAdresa == result.IdAdresa);

            ClassicAssert.IsNotNull(relation);
        }

        [Test]
        public async Task CreateAdresaSiAsigneazaUser_WhenLocationAlreadyExists_ShouldReuseExistingEntities()
        {
            await SeedAddressGraph();
            await SeedUser();

            var dto = CreateValidAdresaDto();
            dto.Bloc = "B";
            dto.Scara = "2";
            dto.Etaj = "5";
            dto.Apartament = "9";

            var result = await _service.CreateAdresaSiAsigneazaUser(dto, 1);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsTrue(result.IdAdresa > 0);

            ClassicAssert.AreEqual(1, _context.Tari.Count());
            ClassicAssert.AreEqual(1, _context.Judete.Count());
            ClassicAssert.AreEqual(1, _context.Localitati.Count());
            ClassicAssert.AreEqual(1, _context.Strazi.Count());
            ClassicAssert.AreEqual(2, _context.Adrese.Count());

            ClassicAssert.AreEqual(1, result.IdStrada);
            ClassicAssert.AreEqual("B", result.Bloc);

            var relation = _context.Adrese_Useri
                .SingleOrDefault(au => au.IdUser == 1 && au.IdAdresa == result.IdAdresa);

            ClassicAssert.IsNotNull(relation);
        }

        [Test]
        public void CreateAdresaSiAsigneazaUser_WhenUserDoesNotExist_ShouldThrowException()
        {
            var dto = CreateValidAdresaDto();

            Assert.ThrowsAsync<Exception>(async () =>
            {
                await _service.CreateAdresaSiAsigneazaUser(dto, 999);
            });
        }

        [Test]
        public async Task UpdateAdresaById_WhenExists_ShouldModifySimpleFields()
        {
            await SeedAddressGraph();

            var dto = new AdresaNestedDTO
            {
                Bloc = "B",
                Scara = "2",
                Etaj = "5",
                Apartament = "9"
            };

            await _service.UpdateAdresaById(1, dto);

            var updated = await _context.Adrese.FindAsync(1);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("B", updated!.Bloc);
            ClassicAssert.AreEqual("2", updated.Scara);
            ClassicAssert.AreEqual("5", updated.Etaj);
            ClassicAssert.AreEqual("9", updated.Apartament);
        }

        [Test]
        public async Task UpdateAdresaById_WhenSomeFieldsAreNull_ShouldKeepOldValues()
        {
            await SeedAddressGraph();

            var dto = new AdresaNestedDTO
            {
                Bloc = null,
                Scara = "9",
                Etaj = null,
                Apartament = "10"
            };

            await _service.UpdateAdresaById(1, dto);

            var updated = await _context.Adrese.FindAsync(1);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("A", updated!.Bloc);
            ClassicAssert.AreEqual("9", updated.Scara);
            ClassicAssert.AreEqual("2", updated.Etaj);
            ClassicAssert.AreEqual("10", updated.Apartament);
        }

        [Test]
        public void UpdateAdresaById_WhenMissing_ShouldThrowKeyNotFoundException()
        {
            var dto = new AdresaNestedDTO
            {
                Bloc = "X"
            };

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.UpdateAdresaById(999, dto);
            });
        }

        [Test]
        public async Task DeleteAdresaById_WhenExists_ShouldRemoveAddress()
        {
            await SeedAddressGraph();

            await _service.DeleteAdresaById(1);

            var deleted = await _context.Adrese.FindAsync(1);

            ClassicAssert.IsNull(deleted);
        }

        [Test]
        public async Task DeleteAdresaById_WhenMissing_ShouldNotThrow()
        {
            await _service.DeleteAdresaById(999);

            ClassicAssert.AreEqual(0, _context.Adrese.Count());
        }

        [Test]
        public async Task AssignAddressToUser_WhenValid_ShouldCreateRelation()
        {
            await SeedAddressGraph();
            await SeedUser();

            await _service.AssignAddressToUser(1, 1);

            var relation = _context.Adrese_Useri
                .SingleOrDefault(au => au.IdUser == 1 && au.IdAdresa == 1);

            ClassicAssert.IsNotNull(relation);
        }

        [Test]
        public async Task AssignAddressToUser_WhenUserMissing_ShouldThrowKeyNotFoundException()
        {
            await SeedAddressGraph();

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.AssignAddressToUser(999, 1);
            });
        }

        [Test]
        public async Task AssignAddressToUser_WhenAddressMissing_ShouldThrowKeyNotFoundException()
        {
            await SeedUser();

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.AssignAddressToUser(1, 999);
            });
        }

        [Test]
        public async Task AssignAddressToUser_WhenAlreadyAssigned_ShouldThrowInvalidOperationException()
        {
            await SeedAddressGraph();
            await SeedUser();

            _context.Adrese_Useri.Add(new Adrese_Useri
            {
                IdUser = 1,
                IdAdresa = 1
            });

            await _context.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _service.AssignAddressToUser(1, 1);
            });
        }

        [Test]
        public async Task RemoveAddressFromUser_WhenAssigned_ShouldRemoveRelation()
        {
            await SeedAddressGraph();
            await SeedUser();

            _context.Adrese_Useri.Add(new Adrese_Useri
            {
                IdUser = 1,
                IdAdresa = 1
            });

            await _context.SaveChangesAsync();

            await _service.RemoveAddressFromUser(1, 1);

            ClassicAssert.AreEqual(0, _context.Adrese_Useri.Count());
        }

        [Test]
        public void RemoveAddressFromUser_WhenRelationMissing_ShouldThrowKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.RemoveAddressFromUser(1, 1);
            });
        }

        [Test]
        public async Task GetAddressesByUser_WhenUserHasAddresses_ShouldReturnAssignedAddresses()
        {
            await SeedAddressGraph();
            await SeedUser();

            _context.Adrese_Useri.Add(new Adrese_Useri
            {
                IdUser = 1,
                IdAdresa = 1
            });

            await _context.SaveChangesAsync();

            var result = await _service.GetAddressesByUser(1);

            ClassicAssert.AreEqual(1, result.Count(a => a.IdAdresa == 1));

            var address = result.Single(a => a.IdAdresa == 1);

            ClassicAssert.IsNotNull(address.Strazi);
            ClassicAssert.IsNotNull(address.Strazi!.Localitati);
            ClassicAssert.IsNotNull(address.Strazi.Localitati!.Judete);
            ClassicAssert.IsNotNull(address.Strazi.Localitati.Judete!.Tari);
            ClassicAssert.AreEqual("Romania", address.Strazi.Localitati.Judete.Tari!.DenumireTara);
        }

        [Test]
        public async Task GetAddressesByUser_WhenUserHasNoAddresses_ShouldReturnEmptyList()
        {
            await SeedUser();

            var result = await _service.GetAddressesByUser(1);

            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetUsersByAddress_WhenAddressHasUsers_ShouldReturnAssignedUsers()
        {
            await SeedAddressGraph();
            await SeedUser();

            _context.Adrese_Useri.Add(new Adrese_Useri
            {
                IdUser = 1,
                IdAdresa = 1
            });

            await _context.SaveChangesAsync();

            var result = await _service.GetUsersByAddress(1);

            ClassicAssert.AreEqual(1, result.Count(u => u.Username == "user"));

            var user = result.Single(u => u.Username == "user");

            ClassicAssert.AreEqual("user", user.Username);
            ClassicAssert.IsNotNull(user.Persoana);
            ClassicAssert.AreEqual("User", user.Persoana!.Nume);
        }

        [Test]
        public async Task GetUsersByAddress_WhenAddressHasNoUsers_ShouldReturnEmptyList()
        {
            await SeedAddressGraph();

            var result = await _service.GetUsersByAddress(1);

            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task GetByIdResponse_WhenAddressExists_ShouldReturnOk()
        {
            await SeedAddressGraph();

            var result = await _service.GetByIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.IsInstanceOf<Adresa>(ok!.Value);

            var value = ok.Value as Adresa;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.AreEqual(1, value!.IdAdresa);
        }

        [Test]
        public async Task GetByIdResponse_WhenAddressMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetByIdResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Address not found", GetAnonymousProperty(notFound.Value!, "error"));
        }

        [Test]
        public async Task CreateAdresaForUserResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.CreateAdresaForUserResponse(1, null!, true);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data.", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task CreateAdresaForUserResponse_WhenModelIsInvalid_ShouldReturnBadRequest()
        {
            var dto = CreateValidAdresaDto();

            var result = await _service.CreateAdresaForUserResponse(1, dto, false);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data.", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task CreateAdresaForUserResponse_WhenValid_ShouldReturnCreatedAtAction()
        {
            await SeedUser();

            var dto = CreateValidAdresaDto();

            var result = await _service.CreateAdresaForUserResponse(1, dto, true);

            var created = result as CreatedAtActionResult;

            ClassicAssert.IsNotNull(created);
            ClassicAssert.AreEqual("GetById", created!.ActionName);
            ClassicAssert.AreEqual("Adresa", created.ControllerName);
            ClassicAssert.IsInstanceOf<Adresa>(created.Value);

            var value = created.Value as Adresa;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.IsTrue(value!.IdAdresa > 0);
        }

        [Test]
        public async Task UpdateAdresaByIdResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.UpdateAdresaByIdResponse(1, null!, true);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task UpdateAdresaByIdResponse_WhenModelIsInvalid_ShouldReturnBadRequest()
        {
            var dto = new AdresaNestedDTO
            {
                Bloc = "X"
            };

            var result = await _service.UpdateAdresaByIdResponse(1, dto, false);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task UpdateAdresaByIdResponse_WhenValid_ShouldReturnOk()
        {
            await SeedAddressGraph();

            var dto = new AdresaNestedDTO
            {
                Bloc = "B"
            };

            var result = await _service.UpdateAdresaByIdResponse(1, dto, true);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("Address updated successfully", GetAnonymousProperty(ok.Value!, "message"));
        }

        [Test]
        public async Task UpdateAdresaByIdResponse_WhenAddressMissing_ShouldReturnNotFound()
        {
            var dto = new AdresaNestedDTO
            {
                Bloc = "B"
            };

            var result = await _service.UpdateAdresaByIdResponse(999, dto, true);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Address not found.", GetAnonymousProperty(notFound.Value!, "error"));
        }

        [Test]
        public async Task UpdateAdresaByIdResponse_WhenUnexpectedErrorOccurs_ShouldReturnInternalServerError()
        {
            await SeedAddressGraph();

            _context.Dispose();

            var dto = new AdresaNestedDTO
            {
                Bloc = "B"
            };

            var result = await _service.UpdateAdresaByIdResponse(1, dto, true);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("An error occurred while updating the address", GetAnonymousProperty(objectResult.Value!, "error"));
        }

        [Test]
        public async Task DeleteAdresaByIdResponse_ShouldReturnNoContent()
        {
            await SeedAddressGraph();

            var result = await _service.DeleteAdresaByIdResponse(1);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);

            var deleted = await _context.Adrese.FindAsync(1);

            ClassicAssert.IsNull(deleted);
        }

        [Test]
        public async Task AssignAddressToUserResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.AssignAddressToUserResponse(null!, true);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data.", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task AssignAddressToUserResponse_WhenModelIsInvalid_ShouldReturnBadRequest()
        {
            var dto = CreateUserAddressDto();

            var result = await _service.AssignAddressToUserResponse(dto, false);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data.", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task AssignAddressToUserResponse_WhenValid_ShouldReturnOk()
        {
            await SeedAddressGraph();
            await SeedUser();

            var dto = CreateUserAddressDto();

            var result = await _service.AssignAddressToUserResponse(dto, true);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("Address assigned to user successfully.", GetAnonymousProperty(ok.Value!, "message"));

            ClassicAssert.AreEqual(1, _context.Adrese_Useri.Count());
        }

        [Test]
        public async Task AssignAddressToUserResponse_WhenUserOrAddressMissing_ShouldReturnNotFound()
        {
            var dto = CreateUserAddressDto();

            var result = await _service.AssignAddressToUserResponse(dto, true);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("User or address not found.", GetAnonymousProperty(notFound.Value!, "error"));
        }

        [Test]
        public async Task AssignAddressToUserResponse_WhenAddressAlreadyAssigned_ShouldReturnInternalServerError()
        {
            await SeedAddressGraph();
            await SeedUser();

            _context.Adrese_Useri.Add(new Adrese_Useri
            {
                IdUser = 1,
                IdAdresa = 1
            });

            await _context.SaveChangesAsync();

            var dto = CreateUserAddressDto();

            var result = await _service.AssignAddressToUserResponse(dto, true);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("An error occurred while assigning the address.", GetAnonymousProperty(objectResult.Value!, "error"));
            ClassicAssert.AreEqual("Address is already assigned to this user.", GetAnonymousProperty(objectResult.Value!, "details"));
        }

        [Test]
        public async Task RemoveAddressFromUserResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.RemoveAddressFromUserResponse(null!, true);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data.", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task RemoveAddressFromUserResponse_WhenModelIsInvalid_ShouldReturnBadRequest()
        {
            var dto = CreateUserAddressDto();

            var result = await _service.RemoveAddressFromUserResponse(dto, false);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data.", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task RemoveAddressFromUserResponse_WhenValid_ShouldReturnOk()
        {
            await SeedAddressGraph();
            await SeedUser();

            _context.Adrese_Useri.Add(new Adrese_Useri
            {
                IdUser = 1,
                IdAdresa = 1
            });

            await _context.SaveChangesAsync();

            var dto = CreateUserAddressDto();

            var result = await _service.RemoveAddressFromUserResponse(dto, true);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("Address removed from user successfully.", GetAnonymousProperty(ok.Value!, "message"));

            ClassicAssert.AreEqual(0, _context.Adrese_Useri.Count());
        }

        [Test]
        public async Task RemoveAddressFromUserResponse_WhenRelationMissing_ShouldReturnNotFound()
        {
            var dto = CreateUserAddressDto();

            var result = await _service.RemoveAddressFromUserResponse(dto, true);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("The address is not assigned to this user.", GetAnonymousProperty(notFound.Value!, "error"));
        }

        [Test]
        public async Task RemoveAddressFromUserResponse_WhenUnexpectedErrorOccurs_ShouldReturnInternalServerError()
        {
            await SeedAddressGraph();
            await SeedUser();

            _context.Adrese_Useri.Add(new Adrese_Useri
            {
                IdUser = 1,
                IdAdresa = 1
            });

            await _context.SaveChangesAsync();

            _context.Dispose();

            var dto = CreateUserAddressDto();

            var result = await _service.RemoveAddressFromUserResponse(dto, true);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("An error occurred while removing the address.", GetAnonymousProperty(objectResult.Value!, "error"));
        }

        [Test]
        public async Task GetAddressesByUserResponse_ShouldReturnOkWithAddresses()
        {
            await SeedAddressGraph();
            await SeedUser();

            _context.Adrese_Useri.Add(new Adrese_Useri
            {
                IdUser = 1,
                IdAdresa = 1
            });

            await _context.SaveChangesAsync();

            var result = await _service.GetAddressesByUserResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var addresses = GetAnonymousProperty(ok.Value!, "addresses") as IEnumerable<Adresa>;

            ClassicAssert.IsNotNull(addresses);
            ClassicAssert.AreEqual(1, addresses!.Count());
            ClassicAssert.AreEqual(1, addresses.First().IdAdresa);
        }

        [Test]
        public async Task GetUsersByAddressResponse_ShouldReturnOkWithUsers()
        {
            await SeedAddressGraph();
            await SeedUser();

            _context.Adrese_Useri.Add(new Adrese_Useri
            {
                IdUser = 1,
                IdAdresa = 1
            });

            await _context.SaveChangesAsync();

            var result = await _service.GetUsersByAddressResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var users = ok.Value as IEnumerable<User>;

            ClassicAssert.IsNotNull(users);
            ClassicAssert.AreEqual(1, users!.Count());
            ClassicAssert.AreEqual("user", users.First().Username);
        }

        [Test]
        public async Task GetSimplifiedAddressesByUserResponse_ShouldReturnOkWithSimplifiedAddresses()
        {
            await SeedAddressGraph();
            await SeedUser();

            _context.Adrese_Useri.Add(new Adrese_Useri
            {
                IdUser = 1,
                IdAdresa = 1
            });

            await _context.SaveChangesAsync();

            var result = await _service.GetSimplifiedAddressesByUserResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var simplifiedAddresses = ok.Value as List<string>;

            ClassicAssert.IsNotNull(simplifiedAddresses);
            ClassicAssert.AreEqual(1, simplifiedAddresses!.Count);
            ClassicAssert.IsTrue(simplifiedAddresses[0].Contains("Tomis"));
            ClassicAssert.IsTrue(simplifiedAddresses[0].Contains("Bloc: A"));
            ClassicAssert.IsTrue(simplifiedAddresses[0].Contains("Constanta"));
            ClassicAssert.IsTrue(simplifiedAddresses[0].Contains("Romania"));
        }
    }
}