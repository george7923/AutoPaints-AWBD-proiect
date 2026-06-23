using Microsoft.AspNetCore.Mvc;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.DataLayer.DTO;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CosController : ControllerBase
    {
        private readonly CosService _cosService;

        public CosController(CosService cosService)
        {
            _cosService = cosService;
        }

        // GET: api/cos/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCartByUserId(int userId)
        {
            return await _cosService.GetCartByUserIdResponse(userId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCartById(int id)
        {
            return await _cosService.GetCartByIdResponse(id);
        }

        [HttpPost("check-or-create/{userId}")]
        public async Task<IActionResult> CheckOrCreateCart(int userId)
        {
            return await _cosService.CheckOrCreateCartResponse(userId);
        }

        [HttpGet("cart-details/{userId}")]
        public async Task<IActionResult> GetCartDetailsByUser(int userId)
        {
            return await _cosService.GetCartDetailsByUserResponse(userId);
        }

        [HttpPut("add-one")]
        public async Task<IActionResult> AddOneSubprodusToCart([FromBody] SubprodusUpdateDTO request)
        {
            return await _cosService.AddOneSubprodusToCartResponse(request);
        }

        [HttpPut("remove-one")]
        public async Task<IActionResult> RemoveOneSubprodusFromCart([FromBody] SubprodusUpdateDTO request)
        {
            return await _cosService.RemoveOneSubprodusFromCartResponse(request);
        }

        [HttpPut("remove-all")]
        public async Task<IActionResult> RemoveAllSubproduseFromCart([FromBody] SubprodusUpdateDTO request)
        {
            return await _cosService.RemoveAllSubproduseFromCartResponse(request);
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteCartByUserId(int userId)
        {
            return await _cosService.DeleteCartByUserIdResponse(userId);
        }

        [HttpPost("addSubproduseToCart")]
        public async Task<IActionResult> AddMultipleSubproduseToCart([FromBody] SubprodusAdaugareDTO dto)
        {
            return await _cosService.AddMultipleSubproduseToCartResponse(dto);
        }

        [HttpGet("cart-content/{userId}")]
        public async Task<IActionResult> GetCartContentByUserId(int userId)
        {
            return await _cosService.GetCartContentByUserIdResponse(userId);
        }
    }
}