using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Data;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly ICardRepository _cardRepository;
        private readonly ApplicationDbContext _context;

        public CardController(ICardRepository cardRepository, ApplicationDbContext context)
        {
            _cardRepository = cardRepository;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCardById(int id)
        {
            var card = await _cardRepository.GetById(id);
            if (card == null)
            {
                return NotFound(new { error = "Card not found" });
            }
            return Ok(card);
        }



        [HttpPost("create")]
        public async Task<IActionResult> CreateCard([FromBody] CardDTO cardDto)
        {
            if (cardDto == null)
                return BadRequest(new { error = "Invalid card data" });

            try
            {
                var newCard = await _cardRepository.CreateCard(cardDto);
                return CreatedAtAction(nameof(GetCardById), new { id = newCard.IdCard }, newCard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error creating card", details = ex.Message });
            }
        }


        [HttpPut("update/{cardId}")]
        public async Task<IActionResult> UpdateCard(int cardId, [FromBody] CardDTO cardDto)
        {
            if (cardDto == null)
                return BadRequest(new { error = "Invalid card data" });

            try
            {
                await _cardRepository.UpdateCard(cardId, cardDto);
                return Ok(new { message = "Card updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error updating card", details = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCardsByUserId(int userId)
        {
            var cards = await _cardRepository.GetCardsByUserId(userId);
            return Ok(cards);
        }


        [HttpDelete("delete/{cardId}")]
        public async Task<IActionResult> DeleteCard(int cardId)
        {
            try
            {
                await _cardRepository.DeleteCard(cardId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error deleting card", details = ex.Message });
            }
        }


        [HttpDelete("user/{userId}/delete-all")]
        public async Task<IActionResult> DeleteAllCardsByUserId(int userId)
        {
            await _cardRepository.DeleteAllCardsByUserId(userId);
            return NoContent();
        }


        [HttpPost("assign")]
        public async Task<IActionResult> AssignCardToUser([FromBody] UserCardDTO userCardDto)
        {
            if (userCardDto == null)
                return BadRequest(new { error = "Invalid data" });

            try
            {
                await _cardRepository.AssignCardToUser(userCardDto.IdUser, userCardDto.IdCard);
                return Ok(new { message = "Card assigned successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error assigning card", details = ex.Message });
            }
        }


        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveCardFromUser([FromBody] UserCardDTO userCardDto)
        {
            if (userCardDto == null)
                return BadRequest(new { error = "Invalid data" });

            try
            {
                await _cardRepository.RemoveCardFromUser(userCardDto.IdUser, userCardDto.IdCard);
                return Ok(new { message = "Card removed successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error removing card", details = ex.Message });
            }
        }

        [HttpDelete("remove-all/{userId}")]
        public async Task<IActionResult> RemoveAllCardsFromUser(int userId)
        {
            try
            {
                await _cardRepository.RemoveAllCardsFromUser(userId);
                return Ok(new { message = "All associated cards have been removed from the user." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while removing the cards.", details = ex.Message });
            }
        }

    }
}
