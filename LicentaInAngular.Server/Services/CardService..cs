using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Utils;
using LicentaInAngular.Server.DataLayer.Models;

namespace LicentaInAngular.Server.Repositories
{
    public class CardService : ICardRepository
    {
        private readonly ApplicationDbContext _context;

        public CardService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // METODE EXISTENTE DIN ICardRepository
        // =========================

        public async Task<Carduri?> GetById(int id)
        {
            var card = await _context.Carduri.FindAsync(id);

            if (card != null)
            {
                card.NumarCard = EncryptionHelper.DecryptCARD(card.NumarCard);
                card.CVV = EncryptionHelper.DecryptCARD(card.CVV);
            }

            return card;
        }

        public async Task<Carduri> CreateCard(CardDTO cardDto)
        {
            var card = new Carduri
            {
                NumarCard = EncryptionHelper.EncryptCARD(cardDto.NumarCard),
                CVV = EncryptionHelper.EncryptCARD(cardDto.CVV),
                DataExpirare = cardDto.DataExpirare
            };

            await _context.Carduri.AddAsync(card);
            await _context.SaveChangesAsync();

            return card;
        }

        public async Task UpdateCard(int cardId, CardDTO cardDto)
        {
            var card = await _context.Carduri.FindAsync(cardId);

            if (card == null)
                throw new KeyNotFoundException("Card not found.");

            card.NumarCard = EncryptionHelper.EncryptCARD(cardDto.NumarCard);
            card.CVV = EncryptionHelper.EncryptCARD(cardDto.CVV);
            card.DataExpirare = cardDto.DataExpirare;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Carduri>> GetCardsByUserId(int userId)
        {
            var cards = await (
                from uc in _context.Useri_Carduri
                join c in _context.Carduri
                    on uc.IdCard equals c.IdCard
                where uc.IdUser == userId
                select new Carduri
                {
                    IdCard = c.IdCard,
                    NumarCard = EncryptionHelper.DecryptCARD(c.NumarCard),
                    CVV = EncryptionHelper.DecryptCARD(c.CVV),
                    DataExpirare = c.DataExpirare
                }
            ).ToListAsync();

            return cards;
        }

        public async Task DeleteCard(int cardId)
        {
            var card = await _context.Carduri.FindAsync(cardId);

            if (card == null)
                throw new KeyNotFoundException("Card not found.");

            _context.Carduri.Remove(card);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllCardsByUserId(int userId)
        {
            var userCards = _context.Useri_Carduri.Where(uc => uc.IdUser == userId);

            _context.Useri_Carduri.RemoveRange(userCards);
            await _context.SaveChangesAsync();
        }

        public async Task AssignCardToUser(int userId, int cardId)
        {
            var user = await (from u in _context.Users
                              where u.IdUser == userId
                              select u).FirstOrDefaultAsync();

            var card = await (from c in _context.Carduri
                              where c.IdCard == cardId
                              select c).FirstOrDefaultAsync();

            if (user == null || card == null)
                throw new KeyNotFoundException("User or Card not found.");

            var alreadyAssigned = await (from uc in _context.Useri_Carduri
                                         where uc.IdUser == userId && uc.IdCard == cardId
                                         select uc).AnyAsync();

            if (alreadyAssigned)
                return;

            var relation = new Useri_Carduri
            {
                IdUser = userId,
                IdCard = cardId,
                DataAdaugarii = DateTime.UtcNow
            };

            _context.Useri_Carduri.Add(relation);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCardFromUser(int userId, int cardId)
        {
            var userCard = await _context.Useri_Carduri
                .FirstOrDefaultAsync(uc => uc.IdUser == userId && uc.IdCard == cardId);

            if (userCard == null)
                throw new KeyNotFoundException("Card not assigned to user.");

            _context.Useri_Carduri.Remove(userCard);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAllCardsFromUser(int userId)
        {
            var userCards = _context.Useri_Carduri.Where(uc => uc.IdUser == userId);

            if (!userCards.Any())
                throw new KeyNotFoundException("No associated cards found for this user.");

            _context.Useri_Carduri.RemoveRange(userCards);
            await _context.SaveChangesAsync();
        }

        // =========================
        // LOGICA MUTATA DIN CardController
        // =========================

        public async Task<IActionResult> GetCardByIdResponse(int id)
        {
            var card = await GetById(id);

            if (card == null)
            {
                return new NotFoundObjectResult(new { error = "Card not found" });
            }

            return new OkObjectResult(card);
        }

        public async Task<IActionResult> CreateCardResponse(CardDTO cardDto)
        {
            if (cardDto == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid card data" });
            }

            try
            {
                var newCard = await CreateCard(cardDto);

                return new CreatedAtActionResult(
                    "GetCardById",
                    "Card",
                    new { id = newCard.IdCard },
                    newCard
                );
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "Error creating card",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> UpdateCardResponse(int cardId, CardDTO cardDto)
        {
            if (cardDto == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid card data" });
            }

            try
            {
                await UpdateCard(cardId, cardDto);

                return new OkObjectResult(new
                {
                    message = "Card updated successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "Error updating card",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> GetCardsByUserIdResponse(int userId)
        {
            var cards = await GetCardsByUserId(userId);
            return new OkObjectResult(cards);
        }

        public async Task<IActionResult> DeleteCardResponse(int cardId)
        {
            try
            {
                await DeleteCard(cardId);
                return new NoContentResult();
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "Error deleting card",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> DeleteAllCardsByUserIdResponse(int userId)
        {
            await DeleteAllCardsByUserId(userId);
            return new NoContentResult();
        }

        public async Task<IActionResult> AssignCardToUserResponse(UserCardDTO userCardDto)
        {
            if (userCardDto == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid data" });
            }

            try
            {
                await AssignCardToUser(userCardDto.IdUser, userCardDto.IdCard);

                return new OkObjectResult(new
                {
                    message = "Card assigned successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "Error assigning card",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> RemoveCardFromUserResponse(UserCardDTO userCardDto)
        {
            if (userCardDto == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid data" });
            }

            try
            {
                await RemoveCardFromUser(userCardDto.IdUser, userCardDto.IdCard);

                return new OkObjectResult(new
                {
                    message = "Card removed successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "Error removing card",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> RemoveAllCardsFromUserResponse(int userId)
        {
            try
            {
                await RemoveAllCardsFromUser(userId);

                return new OkObjectResult(new
                {
                    message = "All associated cards have been removed from the user."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "An error occurred while removing the cards.",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }
    }
}