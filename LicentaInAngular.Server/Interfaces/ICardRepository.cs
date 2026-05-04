using System.Collections.Generic;
using System.Threading.Tasks;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;

namespace LicentaInAngular.Server.Repositories
{
    public interface ICardRepository
    {
        Task<Carduri?> GetById(int id);

        Task<Carduri> CreateCard(CardDTO cardDto);
        Task UpdateCard(int cardId, CardDTO cardDto);
        Task<IEnumerable<Carduri>> GetCardsByUserId(int userId);
        Task DeleteCard(int cardId);
        Task DeleteAllCardsByUserId(int userId);
        Task AssignCardToUser(int userId, int cardId);
        Task RemoveCardFromUser(int userId, int cardId);
        Task RemoveAllCardsFromUser(int userId);

    }

}
