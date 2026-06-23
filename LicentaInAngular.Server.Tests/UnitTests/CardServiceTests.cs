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
    public class CardServiceTests
    {
        private ApplicationDbContext _context = null!;
        private CardService _service = null!;

        [SetUp]
        public void Before()
        {
            _context = TestDbContextFactory.CreateContext();
            _service = new CardService(_context);
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

        private static CardDTO CreateValidCardDto()
        {
            return new CardDTO
            {
                NumarCard = "4444123456789012",
                CVV = "555",
                DataExpirare = new DateTime(2027, 11, 1)
            };
        }

        private static CardDTO CreateUpdatedCardDto()
        {
            return new CardDTO
            {
                NumarCard = "5555123456789012",
                CVV = "777",
                DataExpirare = new DateTime(2028, 12, 1)
            };
        }

        private static UserCardDTO CreateUserCardDto(int userId = 1, int cardId = 1)
        {
            return new UserCardDTO
            {
                IdUser = userId,
                IdCard = cardId
            };
        }

        private async Task SeedUser(int userId = 1, int personId = 1)
        {
            _context.Persoane.Add(new Persoana
            {
                IdPersoana = personId,
                Nume = "User",
                Prenume = "Test",
                Email = $"u{userId}@test.com",
                tipPersoana = "Fizica",
                Telefon = "1",
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

        private async Task<Carduri> SeedCard()
        {
            return await _service.CreateCard(CreateValidCardDto());
        }

        [Test]
        public async Task CreateCard_ShouldPersistEncryptedCard()
        {
            var dto = CreateValidCardDto();

            var result = await _service.CreateCard(dto);

            ClassicAssert.IsTrue(result.IdCard > 0);
            ClassicAssert.AreEqual(1, _context.Carduri.Count());
            ClassicAssert.AreNotEqual("4444123456789012", _context.Carduri.Single().NumarCard);
            ClassicAssert.AreNotEqual("555", _context.Carduri.Single().CVV);
        }

        [Test]
        public async Task GetById_WhenExists_ShouldReturnDecryptedCard()
        {
            var created = await SeedCard();

            var result = await _service.GetById(created.IdCard);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("4444123456789012", result!.NumarCard);
            ClassicAssert.AreEqual("555", result.CVV);
            ClassicAssert.AreEqual(new DateTime(2027, 11, 1), result.DataExpirare);
        }

        [Test]
        public async Task GetById_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetById(999);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task UpdateCard_WhenExists_ShouldModifyCard()
        {
            var created = await SeedCard();

            await _service.UpdateCard(created.IdCard, CreateUpdatedCardDto());

            var updated = await _service.GetById(created.IdCard);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("5555123456789012", updated!.NumarCard);
            ClassicAssert.AreEqual("777", updated.CVV);
            ClassicAssert.AreEqual(new DateTime(2028, 12, 1), updated.DataExpirare);
        }

        [Test]
        public void UpdateCard_WhenMissing_ShouldThrowKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.UpdateCard(999, CreateValidCardDto());
            });
        }

        [Test]
        public async Task DeleteCard_WhenExists_ShouldRemoveCard()
        {
            var card = await SeedCard();

            await _service.DeleteCard(card.IdCard);

            var deleted = await _context.Carduri.FindAsync(card.IdCard);

            ClassicAssert.IsNull(deleted);
            ClassicAssert.AreEqual(0, _context.Carduri.Count());
        }

        [Test]
        public void DeleteCard_WhenMissing_ShouldThrowKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.DeleteCard(999);
            });
        }

        [Test]
        public async Task DeleteAllCardsByUserId_ShouldRemoveOnlyUserRelations()
        {
            _context.Useri_Carduri.AddRange(
                new Useri_Carduri
                {
                    IdUser = 1,
                    IdCard = 1,
                    DataAdaugarii = DateTime.UtcNow
                },
                new Useri_Carduri
                {
                    IdUser = 2,
                    IdCard = 2,
                    DataAdaugarii = DateTime.UtcNow
                }
            );

            await _context.SaveChangesAsync();

            await _service.DeleteAllCardsByUserId(1);

            ClassicAssert.AreEqual(1, _context.Useri_Carduri.Count());
            ClassicAssert.AreEqual(2, _context.Useri_Carduri.Single().IdUser);
        }

        [Test]
        public async Task DeleteAllCardsByUserId_WhenUserHasNoCards_ShouldNotThrow()
        {
            await _service.DeleteAllCardsByUserId(999);

            ClassicAssert.AreEqual(0, _context.Useri_Carduri.Count());
        }

        [Test]
        public async Task AssignCardToUser_WhenValid_ShouldCreateRelation()
        {
            await SeedUser();

            var card = await SeedCard();

            await _service.AssignCardToUser(1, card.IdCard);

            var relation = _context.Useri_Carduri
                .SingleOrDefault(uc => uc.IdUser == 1 && uc.IdCard == card.IdCard);

            ClassicAssert.IsNotNull(relation);
            ClassicAssert.AreEqual(1, relation!.IdUser);
            ClassicAssert.AreEqual(card.IdCard, relation.IdCard);
        }

        [Test]
        public async Task AssignCardToUser_WhenAlreadyAssigned_ShouldNotDuplicateRelation()
        {
            await SeedUser();

            var card = await SeedCard();

            await _service.AssignCardToUser(1, card.IdCard);
            await _service.AssignCardToUser(1, card.IdCard);

            ClassicAssert.AreEqual(1, _context.Useri_Carduri.Count());
        }

        [Test]
        public async Task AssignCardToUser_WhenUserMissing_ShouldThrowKeyNotFoundException()
        {
            var card = await SeedCard();

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.AssignCardToUser(999, card.IdCard);
            });
        }

        [Test]
        public async Task AssignCardToUser_WhenCardMissing_ShouldThrowKeyNotFoundException()
        {
            await SeedUser();

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.AssignCardToUser(1, 999);
            });
        }

        [Test]
        public async Task GetCardsByUserId_WhenUserHasCards_ShouldReturnDecryptedUserCards()
        {
            await SeedUser();

            var card = await SeedCard();

            await _service.AssignCardToUser(1, card.IdCard);

            var result = await _service.GetCardsByUserId(1);

            ClassicAssert.AreEqual(1, result.Count());
            ClassicAssert.AreEqual("4444123456789012", result.First().NumarCard);
            ClassicAssert.AreEqual("555", result.First().CVV);
        }

        [Test]
        public async Task GetCardsByUserId_WhenUserHasNoCards_ShouldReturnEmptyList()
        {
            var result = await _service.GetCardsByUserId(999);

            ClassicAssert.AreEqual(0, result.Count());
        }

        [Test]
        public async Task RemoveCardFromUser_WhenAssigned_ShouldRemoveRelation()
        {
            await SeedUser();

            var card = await SeedCard();

            await _service.AssignCardToUser(1, card.IdCard);

            await _service.RemoveCardFromUser(1, card.IdCard);

            ClassicAssert.AreEqual(0, _context.Useri_Carduri.Count());
        }

        [Test]
        public void RemoveCardFromUser_WhenRelationMissing_ShouldThrowKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.RemoveCardFromUser(1, 1);
            });
        }

        [Test]
        public async Task RemoveAllCardsFromUser_WhenCardsExist_ShouldRemoveRelations()
        {
            await SeedUser();

            var card = await SeedCard();

            await _service.AssignCardToUser(1, card.IdCard);

            await _service.RemoveAllCardsFromUser(1);

            ClassicAssert.AreEqual(0, _context.Useri_Carduri.Count());
        }

        [Test]
        public void RemoveAllCardsFromUser_WhenNoCardsExist_ShouldThrowKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.RemoveAllCardsFromUser(1);
            });
        }

        [Test]
        public async Task GetCardByIdResponse_WhenCardExists_ShouldReturnOk()
        {
            var card = await SeedCard();

            var result = await _service.GetCardByIdResponse(card.IdCard);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.IsInstanceOf<Carduri>(ok.Value);

            var value = ok.Value as Carduri;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.AreEqual("4444123456789012", value!.NumarCard);
            ClassicAssert.AreEqual("555", value.CVV);
        }

        [Test]
        public async Task GetCardByIdResponse_WhenCardMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetCardByIdResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Card not found", GetAnonymousProperty(notFound.Value!, "error"));
        }

        [Test]
        public async Task CreateCardResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.CreateCardResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid card data", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task CreateCardResponse_WhenValid_ShouldReturnCreatedAtAction()
        {
            var result = await _service.CreateCardResponse(CreateValidCardDto());

            var created = result as CreatedAtActionResult;

            ClassicAssert.IsNotNull(created);
            ClassicAssert.AreEqual(201, created!.StatusCode);
            ClassicAssert.AreEqual("GetCardById", created.ActionName);
            ClassicAssert.AreEqual("Card", created.ControllerName);
            ClassicAssert.IsInstanceOf<Carduri>(created.Value);

            var value = created.Value as Carduri;

            ClassicAssert.IsNotNull(value);
            ClassicAssert.IsTrue(value!.IdCard > 0);
        }

        [Test]
        public async Task CreateCardResponse_WhenUnexpectedErrorOccurs_ShouldReturnInternalServerError()
        {
            _context.Dispose();

            var result = await _service.CreateCardResponse(CreateValidCardDto());

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("Error creating card", GetAnonymousProperty(objectResult.Value!, "error"));
        }

        [Test]
        public async Task UpdateCardResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.UpdateCardResponse(1, null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid card data", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task UpdateCardResponse_WhenValid_ShouldReturnOk()
        {
            var card = await SeedCard();

            var result = await _service.UpdateCardResponse(card.IdCard, CreateUpdatedCardDto());

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("Card updated successfully", GetAnonymousProperty(ok.Value!, "message"));

            var updated = await _service.GetById(card.IdCard);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("5555123456789012", updated!.NumarCard);
            ClassicAssert.AreEqual("777", updated.CVV);
        }

        [Test]
        public async Task UpdateCardResponse_WhenCardMissing_ShouldReturnNotFound()
        {
            var result = await _service.UpdateCardResponse(999, CreateUpdatedCardDto());

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Card not found.", GetAnonymousProperty(notFound.Value!, "error"));
        }

        [Test]
        public async Task UpdateCardResponse_WhenUnexpectedErrorOccurs_ShouldReturnInternalServerError()
        {
            var card = await SeedCard();

            _context.Dispose();

            var result = await _service.UpdateCardResponse(card.IdCard, CreateUpdatedCardDto());

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("Error updating card", GetAnonymousProperty(objectResult.Value!, "error"));
        }

        [Test]
        public async Task GetCardsByUserIdResponse_ShouldReturnOkWithCards()
        {
            await SeedUser();

            var card = await SeedCard();

            await _service.AssignCardToUser(1, card.IdCard);

            var result = await _service.GetCardsByUserIdResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var cards = ok.Value as IEnumerable<Carduri>;

            ClassicAssert.IsNotNull(cards);
            ClassicAssert.AreEqual(1, cards!.Count());
            ClassicAssert.AreEqual("4444123456789012", cards.First().NumarCard);
            ClassicAssert.AreEqual("555", cards.First().CVV);
        }

        [Test]
        public async Task DeleteCardResponse_WhenCardExists_ShouldReturnNoContent()
        {
            var card = await SeedCard();

            var result = await _service.DeleteCardResponse(card.IdCard);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
            ClassicAssert.AreEqual(0, _context.Carduri.Count());
        }

        [Test]
        public async Task DeleteCardResponse_WhenCardMissing_ShouldReturnNotFound()
        {
            var result = await _service.DeleteCardResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Card not found.", GetAnonymousProperty(notFound.Value!, "error"));
        }

        [Test]
        public async Task DeleteCardResponse_WhenUnexpectedErrorOccurs_ShouldReturnInternalServerError()
        {
            var card = await SeedCard();

            _context.Dispose();

            var result = await _service.DeleteCardResponse(card.IdCard);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("Error deleting card", GetAnonymousProperty(objectResult.Value!, "error"));
        }

        [Test]
        public async Task DeleteAllCardsByUserIdResponse_ShouldReturnNoContent()
        {
            _context.Useri_Carduri.AddRange(
                new Useri_Carduri
                {
                    IdUser = 1,
                    IdCard = 1,
                    DataAdaugarii = DateTime.UtcNow
                },
                new Useri_Carduri
                {
                    IdUser = 2,
                    IdCard = 2,
                    DataAdaugarii = DateTime.UtcNow
                }
            );

            await _context.SaveChangesAsync();

            var result = await _service.DeleteAllCardsByUserIdResponse(1);

            var noContent = result as NoContentResult;

            ClassicAssert.IsNotNull(noContent);
            ClassicAssert.AreEqual(204, noContent!.StatusCode);
            ClassicAssert.AreEqual(1, _context.Useri_Carduri.Count());
            ClassicAssert.AreEqual(2, _context.Useri_Carduri.Single().IdUser);
        }

        [Test]
        public async Task AssignCardToUserResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.AssignCardToUserResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid data", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task AssignCardToUserResponse_WhenValid_ShouldReturnOk()
        {
            await SeedUser();

            var card = await SeedCard();

            var dto = CreateUserCardDto(1, card.IdCard);

            var result = await _service.AssignCardToUserResponse(dto);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("Card assigned successfully", GetAnonymousProperty(ok.Value!, "message"));
            ClassicAssert.AreEqual(1, _context.Useri_Carduri.Count());
        }

        [Test]
        public async Task AssignCardToUserResponse_WhenAlreadyAssigned_ShouldReturnOkAndNotDuplicate()
        {
            await SeedUser();

            var card = await SeedCard();

            await _service.AssignCardToUser(1, card.IdCard);

            var dto = CreateUserCardDto(1, card.IdCard);

            var result = await _service.AssignCardToUserResponse(dto);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("Card assigned successfully", GetAnonymousProperty(ok.Value!, "message"));
            ClassicAssert.AreEqual(1, _context.Useri_Carduri.Count());
        }

        [Test]
        public async Task AssignCardToUserResponse_WhenUserOrCardMissing_ShouldReturnNotFound()
        {
            var dto = CreateUserCardDto(1, 1);

            var result = await _service.AssignCardToUserResponse(dto);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("User or Card not found.", GetAnonymousProperty(notFound.Value!, "error"));
        }

        [Test]
        public async Task AssignCardToUserResponse_WhenUnexpectedErrorOccurs_ShouldReturnInternalServerError()
        {
            _context.Dispose();

            var dto = CreateUserCardDto(1, 1);

            var result = await _service.AssignCardToUserResponse(dto);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("Error assigning card", GetAnonymousProperty(objectResult.Value!, "error"));
        }

        [Test]
        public async Task RemoveCardFromUserResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.RemoveCardFromUserResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid data", GetAnonymousProperty(badRequest.Value!, "error"));
        }

        [Test]
        public async Task RemoveCardFromUserResponse_WhenValid_ShouldReturnOk()
        {
            await SeedUser();

            var card = await SeedCard();

            await _service.AssignCardToUser(1, card.IdCard);

            var dto = CreateUserCardDto(1, card.IdCard);

            var result = await _service.RemoveCardFromUserResponse(dto);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("Card removed successfully", GetAnonymousProperty(ok.Value!, "message"));
            ClassicAssert.AreEqual(0, _context.Useri_Carduri.Count());
        }

        [Test]
        public async Task RemoveCardFromUserResponse_WhenRelationMissing_ShouldReturnNotFound()
        {
            var dto = CreateUserCardDto(1, 1);

            var result = await _service.RemoveCardFromUserResponse(dto);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("Card not assigned to user.", GetAnonymousProperty(notFound.Value!, "error"));
        }

        [Test]
        public async Task RemoveCardFromUserResponse_WhenUnexpectedErrorOccurs_ShouldReturnInternalServerError()
        {
            _context.Dispose();

            var dto = CreateUserCardDto(1, 1);

            var result = await _service.RemoveCardFromUserResponse(dto);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("Error removing card", GetAnonymousProperty(objectResult.Value!, "error"));
        }

        [Test]
        public async Task RemoveAllCardsFromUserResponse_WhenCardsExist_ShouldReturnOk()
        {
            await SeedUser();

            var card = await SeedCard();

            await _service.AssignCardToUser(1, card.IdCard);

            var result = await _service.RemoveAllCardsFromUserResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual(
                "All associated cards have been removed from the user.",
                GetAnonymousProperty(ok.Value!, "message")
            );
            ClassicAssert.AreEqual(0, _context.Useri_Carduri.Count());
        }

        [Test]
        public async Task RemoveAllCardsFromUserResponse_WhenNoCardsExist_ShouldReturnNotFound()
        {
            var result = await _service.RemoveAllCardsFromUserResponse(1);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual(
                "No associated cards found for this user.",
                GetAnonymousProperty(notFound.Value!, "error")
            );
        }

        [Test]
        public async Task RemoveAllCardsFromUserResponse_WhenUnexpectedErrorOccurs_ShouldReturnInternalServerError()
        {
            _context.Dispose();

            var result = await _service.RemoveAllCardsFromUserResponse(1);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual(
                "An error occurred while removing the cards.",
                GetAnonymousProperty(objectResult.Value!, "error")
            );
        }
    }
}