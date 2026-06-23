using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly CardService _cardService;

        public CardController(CardService cardService)
        {
            _cardService = cardService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCardById(int id)
        {
            return await _cardService.GetCardByIdResponse(id);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCard([FromBody] CardDTO cardDto)
        {
            return await _cardService.CreateCardResponse(cardDto);
        }

        [HttpPut("update/{cardId}")]
        public async Task<IActionResult> UpdateCard(int cardId, [FromBody] CardDTO cardDto)
        {
            return await _cardService.UpdateCardResponse(cardId, cardDto);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCardsByUserId(int userId)
        {
            return await _cardService.GetCardsByUserIdResponse(userId);
        }

        [HttpDelete("delete/{cardId}")]
        public async Task<IActionResult> DeleteCard(int cardId)
        {
            return await _cardService.DeleteCardResponse(cardId);
        }

        [HttpDelete("user/{userId}/delete-all")]
        public async Task<IActionResult> DeleteAllCardsByUserId(int userId)
        {
            return await _cardService.DeleteAllCardsByUserIdResponse(userId);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignCardToUser([FromBody] UserCardDTO userCardDto)
        {
            return await _cardService.AssignCardToUserResponse(userCardDto);
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveCardFromUser([FromBody] UserCardDTO userCardDto)
        {
            return await _cardService.RemoveCardFromUserResponse(userCardDto);
        }

        [HttpDelete("remove-all/{userId}")]
        public async Task<IActionResult> RemoveAllCardsFromUser(int userId)
        {
            return await _cardService.RemoveAllCardsFromUserResponse(userId);
        }
    }
}