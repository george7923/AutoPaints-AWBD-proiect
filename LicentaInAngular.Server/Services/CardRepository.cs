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
    public class CardRepository : ICardRepository
    {
        private readonly ApplicationDbContext _context;

        public CardRepository(ApplicationDbContext context)
        {
            _context = context;
        }


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
            var userCard = await _context.Useri_Carduri.FirstOrDefaultAsync(uc => uc.IdUser == userId && uc.IdCard == cardId);
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


    }
}
