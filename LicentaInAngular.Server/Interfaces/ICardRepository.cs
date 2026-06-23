using System.Collections.Generic;
using System.Threading.Tasks;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using Microsoft.AspNetCore.Mvc;

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

        // Metode Response mutate din CardController
        Task<IActionResult> GetCardByIdResponse(int id);

        Task<IActionResult> CreateCardResponse(CardDTO cardDto);

        Task<IActionResult> UpdateCardResponse(int cardId, CardDTO cardDto);

        Task<IActionResult> GetCardsByUserIdResponse(int userId);

        Task<IActionResult> DeleteCardResponse(int cardId);

        Task<IActionResult> DeleteAllCardsByUserIdResponse(int userId);

        Task<IActionResult> AssignCardToUserResponse(UserCardDTO userCardDto);

        Task<IActionResult> RemoveCardFromUserResponse(UserCardDTO userCardDto);

        Task<IActionResult> RemoveAllCardsFromUserResponse(int userId);
    }
}
