using System;
using System.Collections;
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
    public class CosServiceTests
    {
        private ApplicationDbContext _context = null!;
        private CosService _service = null!;

        [SetUp]
        public void Before()
        {
            _context = TestDbContextFactory.CreateContext();
            _service = new CosService(_context);
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

        private async Task SeedUser(int userId = 1, int personId = 1)
        {
            _context.Persoane.Add(new Persoana
            {
                IdPersoana = personId,
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
                IdPersoana = personId
            });

            await _context.SaveChangesAsync();
        }

        private async Task SeedCart(int cartId = 1, int userId = 1, string code = "CART")
        {
            _context.Cosuri.Add(new Cos
            {
                idCos = cartId,
                CodUnic = code,
                IdUser = userId,
                DataCreare = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        private async Task SeedCategoryAndProducts()
        {
            _context.Categorii.Add(new Categorii
            {
                IdCategorie = 1,
                DenumireCategorie = "Vopsea",
                DescriereCategorie = "Categorie test"
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
                    IdProdus = 1,
                    Pret = 30,
                    DataInceput = DateTime.UtcNow.AddDays(-1)
                },
                new Preturi_Produs
                {
                    idPP = 3,
                    IdProdus = 2,
                    Pret = 40,
                    DataInceput = DateTime.UtcNow.AddDays(-1)
                }
            );

            await _context.SaveChangesAsync();
        }

        private async Task SeedSubproductsForCart(int cartId = 1)
        {
            _context.SubProduse.AddRange(
                new Subprodus
                {
                    IdSubprodus = 1,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = cartId
                },
                new Subprodus
                {
                    IdSubprodus = 2,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = cartId
                },
                new Subprodus
                {
                    IdSubprodus = 3,
                    IdProdus = 2,
                    Valabil = true,
                    idCos = cartId
                },
                new Subprodus
                {
                    IdSubprodus = 4,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = null
                }
            );

            await _context.SaveChangesAsync();
        }

        private static SubprodusUpdateDTO CreateSubprodusUpdateDto(int productId = 1, int cartId = 1)
        {
            return new SubprodusUpdateDTO
            {
                IdProdus = productId,
                idCos = cartId
            };
        }

        private static SubprodusAdaugareDTO CreateSubprodusAdaugareDto(
            int productId = 1,
            int quantity = 2,
            int userId = 1,
            int cartId = 1)
        {
            return new SubprodusAdaugareDTO
            {
                IdProdus = productId,
                Quantity = quantity,
                IdUser = userId,
                idCos = cartId
            };
        }

        [Test]
        public async Task CreateCart_ShouldPersistCart()
        {
            await _service.CreateCart(new Cos
            {
                CodUnic = "ABC",
                IdUser = 1,
                DataCreare = DateTime.UtcNow
            });

            ClassicAssert.AreEqual(1, _context.Cosuri.Count(c => c.CodUnic == "ABC"));
        }

        [Test]
        public async Task GetCartByUserId_WhenExists_ShouldReturnCart()
        {
            await SeedUser(1, 1);
            await SeedCart(10, 1, "C10");

            var result = await _service.GetCartByUserId(1);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("C10", result!.CodUnic);
            ClassicAssert.IsNotNull(result.User);
            ClassicAssert.AreEqual("user1", result.User!.Username);
        }

        [Test]
        public async Task GetCartByUserId_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetCartByUserId(999);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task GetProductsByUserId_WhenCartMissing_ShouldReturnEmptyList()
        {
            var result = await _service.GetProductsByUserId(999);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetProductsByUserId_ShouldGroupProductsByQuantity()
        {
            await SeedCart(1, 1, "C");
            await SeedSubproductsForCart(1);

            var result = await _service.GetProductsByUserId(1);

            ClassicAssert.AreEqual(2, result.Count);
            ClassicAssert.AreEqual(2, result.First(x => x.IdProdus == 1).Quantity);
            ClassicAssert.AreEqual(1, result.First(x => x.IdProdus == 2).Quantity);
        }

        [Test]
        public async Task ClearCart_WhenCartMissing_ShouldNotThrow()
        {
            await _service.ClearCart(999);

            ClassicAssert.AreEqual(0, _context.SubProduse.Count());
        }

        [Test]
        public async Task ClearCart_WhenCartExists_ShouldSetSubproductsCartToNull()
        {
            await SeedCart(2, 2, "C2");

            _context.SubProduse.AddRange(
                new Subprodus
                {
                    IdSubprodus = 5,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = 2
                },
                new Subprodus
                {
                    IdSubprodus = 6,
                    IdProdus = 2,
                    Valabil = true,
                    idCos = 2
                }
            );

            await _context.SaveChangesAsync();

            await _service.ClearCart(2);

            ClassicAssert.IsTrue(_context.SubProduse.All(sp => sp.idCos == null));
        }

        [Test]
        public async Task GetById_WhenExists_ShouldReturnCartWithUser()
        {
            await SeedUser(3, 3);
            await SeedCart(3, 3, "C3");

            var result = await _service.GetById(3);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("C3", result!.CodUnic);
            ClassicAssert.IsNotNull(result.User);
            ClassicAssert.AreEqual("user3", result.User!.Username);
        }

        [Test]
        public async Task GetById_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetById(999);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task GetAllCarts_ShouldReturnAllCarts()
        {
            await SeedUser(1, 1);
            await SeedUser(2, 2);
            await SeedCart(1, 1, "C1");
            await SeedCart(2, 2, "C2");

            var result = await _service.GetAllCarts();

            ClassicAssert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetAllCarts_WhenEmpty_ShouldReturnEmptyList()
        {
            var result = await _service.GetAllCarts();

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task UpdateCart_ShouldModifyCart()
        {
            await SeedCart(3, 3, "Old");

            _context.ChangeTracker.Clear();

            await _service.UpdateCart(new Cos
            {
                idCos = 3,
                CodUnic = "New",
                IdUser = 3,
                DataCreare = DateTime.UtcNow
            });

            var updated = await _context.Cosuri.FindAsync(3);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("New", updated!.CodUnic);
        }

        [Test]
        public async Task DeleteCart_WhenExists_ShouldRemoveCart()
        {
            await SeedCart(4, 4, "Del");

            await _service.DeleteCart(4);

            var deleted = await _context.Cosuri.FindAsync(4);

            ClassicAssert.IsNull(deleted);
        }

        [Test]
        public async Task DeleteCart_WhenMissing_ShouldNotThrow()
        {
            await _service.DeleteCart(999);

            ClassicAssert.AreEqual(0, _context.Cosuri.Count());
        }

        [Test]
        public async Task DeleteOldCarts_ShouldRemoveOnlyOldCarts()
        {
            _context.Cosuri.AddRange(
                new Cos
                {
                    idCos = 8,
                    CodUnic = "Old",
                    IdUser = 8,
                    DataCreare = DateTime.UtcNow.AddDays(-10)
                },
                new Cos
                {
                    idCos = 9,
                    CodUnic = "New",
                    IdUser = 9,
                    DataCreare = DateTime.UtcNow
                }
            );

            await _context.SaveChangesAsync();

            await _service.DeleteOldCarts(DateTime.UtcNow.AddDays(-1));

            ClassicAssert.AreEqual(1, _context.Cosuri.Count());
            ClassicAssert.AreEqual("New", _context.Cosuri.Single().CodUnic);
        }

        [Test]
        public async Task DeleteOldCarts_WhenNoOldCarts_ShouldNotRemoveAnything()
        {
            _context.Cosuri.Add(new Cos
            {
                idCos = 9,
                CodUnic = "New",
                IdUser = 9,
                DataCreare = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await _service.DeleteOldCarts(DateTime.UtcNow.AddDays(-1));

            ClassicAssert.AreEqual(1, _context.Cosuri.Count());
        }

        [Test]
        public async Task GetCartByUserIdResponse_WhenCartExists_ShouldReturnOk()
        {
            await SeedUser(1, 1);
            await SeedCart(1, 1, "C1");

            var result = await _service.GetCartByUserIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.IsInstanceOf<Cos>(ok.Value);

            var cart = ok.Value as Cos;

            ClassicAssert.IsNotNull(cart);
            ClassicAssert.AreEqual("C1", cart!.CodUnic);
        }

        [Test]
        public async Task GetCartByUserIdResponse_WhenCartMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetCartByUserIdResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual(
                "Cart not found for the given user.",
                GetAnonymousProperty(notFound.Value!, "message")
            );
        }

        [Test]
        public async Task GetCartByIdResponse_WhenCartExists_ShouldReturnOk()
        {
            await SeedCart(1, 1, "C1");

            var result = await _service.GetCartByIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.IsInstanceOf<Cos>(ok.Value);

            var cart = ok.Value as Cos;

            ClassicAssert.IsNotNull(cart);
            ClassicAssert.AreEqual(1, cart!.idCos);
        }

        [Test]
        public async Task GetCartByIdResponse_WhenCartMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetCartByIdResponse(999);

            ClassicAssert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task CheckOrCreateCartResponse_WhenUserIdInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.CheckOrCreateCartResponse(0);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid user ID.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task CheckOrCreateCartResponse_WhenUserMissing_ShouldReturnNotFound()
        {
            var result = await _service.CheckOrCreateCartResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual(
                "Utilizator inexistent.Nu se poate crea un coș.",
                GetAnonymousProperty(notFound.Value!, "message")
            );
        }

        [Test]
        public async Task CheckOrCreateCartResponse_WhenUserAlreadyHasCart_ShouldReturnConflict()
        {
            await SeedUser(1, 1);
            await SeedCart(10, 1, "C10");

            var result = await _service.CheckOrCreateCartResponse(1);

            var conflict = result as ConflictObjectResult;

            ClassicAssert.IsNotNull(conflict);
            ClassicAssert.AreEqual(409, conflict!.StatusCode);
            ClassicAssert.AreEqual("Utilizatorul are deja un coș.", GetAnonymousProperty(conflict.Value!, "message"));
            ClassicAssert.AreEqual(10, GetAnonymousProperty(conflict.Value!, "cartId"));
        }

        [Test]
        public async Task CheckOrCreateCartResponse_WhenValid_ShouldCreateCart()
        {
            await SeedUser(1, 1);

            var result = await _service.CheckOrCreateCartResponse(1);

            var created = result as CreatedAtActionResult;

            ClassicAssert.IsNotNull(created);
            ClassicAssert.AreEqual(201, created!.StatusCode);
            ClassicAssert.AreEqual("GetCartByUserId", created.ActionName);
            ClassicAssert.AreEqual("Cos", created.ControllerName);
            ClassicAssert.IsInstanceOf<Cos>(created.Value);
            ClassicAssert.AreEqual(1, _context.Cosuri.Count());
        }

        [Test]
        public async Task GetCartDetailsByUserResponse_WhenCartMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetCartDetailsByUserResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual(
                "Cart not found for the given user.",
                GetAnonymousProperty(notFound.Value!, "message")
            );
        }

        [Test]
        public async Task GetCartDetailsByUserResponse_WhenCartExists_ShouldReturnCartAndGroupedItems()
        {
            await SeedCart(1, 1, "C1");
            await SeedCategoryAndProducts();
            await SeedSubproductsForCart(1);

            var result = await _service.GetCartDetailsByUserResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var cart = GetAnonymousProperty(ok.Value!, "Cart") as Cos;
            var items = AnonymousList(GetAnonymousProperty(ok.Value!, "Items")!);

            ClassicAssert.IsNotNull(cart);
            ClassicAssert.AreEqual(1, cart!.idCos);
            ClassicAssert.AreEqual(2, items.Count);

            var productOneItem = items.First(x => (int)GetAnonymousProperty(x, "IdProdus")! == 1);

            ClassicAssert.AreEqual(2, GetAnonymousProperty(productOneItem, "Quantity"));
            ClassicAssert.IsInstanceOf<Produs>(GetAnonymousProperty(productOneItem, "Produs"));
        }

        [Test]
        public async Task AddOneSubprodusToCartResponse_WhenRequestIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.AddOneSubprodusToCartResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddOneSubprodusToCartResponse_WhenProductIdInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.AddOneSubprodusToCartResponse(CreateSubprodusUpdateDto(0, 1));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddOneSubprodusToCartResponse_WhenCartIdInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.AddOneSubprodusToCartResponse(CreateSubprodusUpdateDto(1, 0));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddOneSubprodusToCartResponse_WhenNoAvailableSubproduct_ShouldReturnNotFound()
        {
            await SeedCart(1, 1, "C1");

            var result = await _service.AddOneSubprodusToCartResponse(CreateSubprodusUpdateDto(1, 1));

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual(
                "No available subproduct for this product.",
                GetAnonymousProperty(notFound.Value!, "message")
            );
        }

        [Test]
        public async Task AddOneSubprodusToCartResponse_WhenAvailableSubproductExists_ShouldAssignItToCart()
        {
            await SeedCart(1, 1, "C1");

            _context.SubProduse.Add(new Subprodus
            {
                IdSubprodus = 1,
                IdProdus = 1,
                Valabil = true,
                idCos = null
            });

            await _context.SaveChangesAsync();

            var result = await _service.AddOneSubprodusToCartResponse(CreateSubprodusUpdateDto(1, 1));

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("One subproduct added to cart.", GetAnonymousProperty(ok.Value!, "message"));
            ClassicAssert.AreEqual(1, _context.SubProduse.Single().idCos);
        }

        [Test]
        public async Task RemoveOneSubprodusFromCartResponse_WhenRequestIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.RemoveOneSubprodusFromCartResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task RemoveOneSubprodusFromCartResponse_WhenNoSubproductInCart_ShouldReturnNotFound()
        {
            await SeedCart(1, 1, "C1");

            var result = await _service.RemoveOneSubprodusFromCartResponse(CreateSubprodusUpdateDto(1, 1));

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual(
                "No subproduct found in the cart for this product.",
                GetAnonymousProperty(notFound.Value!, "message")
            );
        }

        [Test]
        public async Task RemoveOneSubprodusFromCartResponse_WhenSubproductExists_ShouldUnassignOne()
        {
            await SeedCart(1, 1, "C1");

            _context.SubProduse.AddRange(
                new Subprodus
                {
                    IdSubprodus = 1,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = 1
                },
                new Subprodus
                {
                    IdSubprodus = 2,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = 1
                }
            );

            await _context.SaveChangesAsync();

            var result = await _service.RemoveOneSubprodusFromCartResponse(CreateSubprodusUpdateDto(1, 1));

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("One subproduct removed from cart.", GetAnonymousProperty(ok.Value!, "message"));
            ClassicAssert.AreEqual(1, _context.SubProduse.Count(sp => sp.idCos == null));
            ClassicAssert.AreEqual(1, _context.SubProduse.Count(sp => sp.idCos == 1));
        }

        [Test]
        public async Task RemoveAllSubproduseFromCartResponse_WhenRequestIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.RemoveAllSubproduseFromCartResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task RemoveAllSubproduseFromCartResponse_WhenNoSubproductsInCart_ShouldReturnNotFound()
        {
            await SeedCart(1, 1, "C1");

            var result = await _service.RemoveAllSubproduseFromCartResponse(CreateSubprodusUpdateDto(1, 1));

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual(
                "No subproducts found in the cart for this product.",
                GetAnonymousProperty(notFound.Value!, "message")
            );
        }

        [Test]
        public async Task RemoveAllSubproduseFromCartResponse_WhenSubproductsExist_ShouldUnassignAllForProduct()
        {
            await SeedCart(1, 1, "C1");

            _context.SubProduse.AddRange(
                new Subprodus
                {
                    IdSubprodus = 1,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = 1
                },
                new Subprodus
                {
                    IdSubprodus = 2,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = 1
                },
                new Subprodus
                {
                    IdSubprodus = 3,
                    IdProdus = 2,
                    Valabil = true,
                    idCos = 1
                }
            );

            await _context.SaveChangesAsync();

            var result = await _service.RemoveAllSubproduseFromCartResponse(CreateSubprodusUpdateDto(1, 1));

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual(
                "All subproducts for the product removed from cart.",
                GetAnonymousProperty(ok.Value!, "message")
            );

            ClassicAssert.AreEqual(2, _context.SubProduse.Count(sp => sp.IdProdus == 1 && sp.idCos == null));
            ClassicAssert.AreEqual(1, _context.SubProduse.Count(sp => sp.IdProdus == 2 && sp.idCos == 1));
        }

        [Test]
        public async Task DeleteCartByUserIdResponse_WhenUserMissing_ShouldReturnNotFound()
        {
            var result = await _service.DeleteCartByUserIdResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("User not found.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task DeleteCartByUserIdResponse_WhenCartMissing_ShouldReturnNotFound()
        {
            await SeedUser(1, 1);

            var result = await _service.DeleteCartByUserIdResponse(1);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Cart not found for user.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task DeleteCartByUserIdResponse_WhenCartExists_ShouldDeleteCartAndReturnOk()
        {
            await SeedUser(1, 1);
            await SeedCart(1, 1, "C1");

            var result = await _service.DeleteCartByUserIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("Cart deleted successfully.", GetAnonymousProperty(ok.Value!, "message"));
            ClassicAssert.AreEqual(0, _context.Cosuri.Count());
        }

        [Test]
        public async Task AddMultipleSubproduseToCartResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.AddMultipleSubproduseToCartResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Datele trimise sunt invalide.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddMultipleSubproduseToCartResponse_WhenProductIdInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.AddMultipleSubproduseToCartResponse(CreateSubprodusAdaugareDto(0, 1, 1, 1));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Datele trimise sunt invalide.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddMultipleSubproduseToCartResponse_WhenQuantityInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.AddMultipleSubproduseToCartResponse(CreateSubprodusAdaugareDto(1, 0, 1, 1));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Datele trimise sunt invalide.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddMultipleSubproduseToCartResponse_WhenUserIdInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.AddMultipleSubproduseToCartResponse(CreateSubprodusAdaugareDto(1, 1, 0, 1));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Datele trimise sunt invalide.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddMultipleSubproduseToCartResponse_WhenCartIdInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.AddMultipleSubproduseToCartResponse(CreateSubprodusAdaugareDto(1, 1, 1, 0));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Datele trimise sunt invalide.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddMultipleSubproduseToCartResponse_WhenProductMissing_ShouldReturnNotFound()
        {
            await SeedCart(1, 1, "C1");

            var result = await _service.AddMultipleSubproduseToCartResponse(CreateSubprodusAdaugareDto(1, 1, 1, 1));

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Produsul nu există.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task AddMultipleSubproduseToCartResponse_WhenCartMissing_ShouldReturnNotFound()
        {
            await SeedCategoryAndProducts();

            var result = await _service.AddMultipleSubproduseToCartResponse(CreateSubprodusAdaugareDto(1, 1, 1, 999));

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Coșul nu există.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task AddMultipleSubproduseToCartResponse_WhenNotEnoughAvailableSubproducts_ShouldReturnBadRequest()
        {
            await SeedCart(1, 1, "C1");
            await SeedCategoryAndProducts();

            _context.SubProduse.Add(new Subprodus
            {
                IdSubprodus = 1,
                IdProdus = 1,
                Valabil = true,
                idCos = null
            });

            await _context.SaveChangesAsync();

            var result = await _service.AddMultipleSubproduseToCartResponse(CreateSubprodusAdaugareDto(1, 2, 1, 1));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Nu există suficiente subproduse disponibile.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task AddMultipleSubproduseToCartResponse_WhenValid_ShouldAssignRequestedQuantity()
        {
            await SeedCart(1, 1, "C1");
            await SeedCategoryAndProducts();

            _context.SubProduse.AddRange(
                new Subprodus
                {
                    IdSubprodus = 1,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = null
                },
                new Subprodus
                {
                    IdSubprodus = 2,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = null
                },
                new Subprodus
                {
                    IdSubprodus = 3,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = null
                }
            );

            await _context.SaveChangesAsync();

            var result = await _service.AddMultipleSubproduseToCartResponse(CreateSubprodusAdaugareDto(1, 2, 1, 1));

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("2 subproduse adăugate în coș.", GetAnonymousProperty(ok.Value!, "message"));
            ClassicAssert.AreEqual(2, _context.SubProduse.Count(sp => sp.idCos == 1));
            ClassicAssert.AreEqual(1, _context.SubProduse.Count(sp => sp.idCos == null));
        }

        [Test]
        public async Task GetCartContentByUserIdResponse_WhenUserHasNoCart_ShouldReturnNotFound()
        {
            var result = await _service.GetCartContentByUserIdResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("User does not have a cart.", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task GetCartContentByUserIdResponse_WhenCartExists_ShouldReturnGroupedProducts()
        {
            await SeedCart(1, 1, "C1");
            await SeedCategoryAndProducts();

            _context.SubProduse.AddRange(
                new Subprodus
                {
                    IdSubprodus = 1,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = 1
                },
                new Subprodus
                {
                    IdSubprodus = 2,
                    IdProdus = 1,
                    Valabil = true,
                    idCos = 1
                },
                new Subprodus
                {
                    IdSubprodus = 3,
                    IdProdus = 2,
                    Valabil = true,
                    idCos = 1
                }
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetCartContentByUserIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var cart = GetAnonymousProperty(ok.Value!, "cart") as Cos;
            var products = AnonymousList(GetAnonymousProperty(ok.Value!, "products")!);

            ClassicAssert.IsNotNull(cart);
            ClassicAssert.AreEqual(1, cart!.idCos);
            ClassicAssert.AreEqual(2, products.Count);

            var productOne = products.First(x => (int)GetAnonymousProperty(x, "IdProdus")! == 1);
            var productTwo = products.First(x => (int)GetAnonymousProperty(x, "IdProdus")! == 2);

            ClassicAssert.AreEqual("Produs 1", GetAnonymousProperty(productOne, "Nume"));
            ClassicAssert.AreEqual(30m, GetAnonymousProperty(productOne, "Pret"));
            ClassicAssert.AreEqual("Vopsea", GetAnonymousProperty(productOne, "Categorie"));
            ClassicAssert.AreEqual(false, GetAnonymousProperty(productOne, "EsteSpray"));
            ClassicAssert.AreEqual("N/A", GetAnonymousProperty(productOne, "CodCuloare"));
            ClassicAssert.AreEqual(2, GetAnonymousProperty(productOne, "Cantitatea"));

            ClassicAssert.AreEqual("Produs 2", GetAnonymousProperty(productTwo, "Nume"));
            ClassicAssert.AreEqual(40m, GetAnonymousProperty(productTwo, "Pret"));
            ClassicAssert.AreEqual(1, GetAnonymousProperty(productTwo, "Cantitatea"));
        }

        [Test]
        public async Task GetCartContentByUserIdResponse_WhenProductHasNoCategoryOrPrice_ShouldUseFallbackValues()
        {
            await SeedCart(1, 1, "C1");

            _context.Products.Add(new Produs
            {
                IdProdus = 10,
                Nume = "Fara categorie",
                Descriere = "D",
                EsteSpray = false,
                Valabil = true,
                IdCategorie = 999
            });

            _context.SubProduse.Add(new Subprodus
            {
                IdSubprodus = 10,
                IdProdus = 10,
                Valabil = true,
                idCos = 1
            });

            await _context.SaveChangesAsync();

            var result = await _service.GetCartContentByUserIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);

            var products = AnonymousList(GetAnonymousProperty(ok!.Value!, "products")!);
            var product = products.Single();

            ClassicAssert.AreEqual("Necunoscut", GetAnonymousProperty(product, "Categorie"));
            ClassicAssert.AreEqual(0m, GetAnonymousProperty(product, "Pret"));
            ClassicAssert.AreEqual("N/A", GetAnonymousProperty(product, "CodCuloare"));
            ClassicAssert.AreEqual(1, GetAnonymousProperty(product, "Cantitatea"));
        }
    }
}