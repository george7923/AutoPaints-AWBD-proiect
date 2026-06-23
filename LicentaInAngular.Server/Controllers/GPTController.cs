using LicentaInAngular.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GPTController : ControllerBase
    {
        private readonly IGPTService _gptService;

        public GPTController(IGPTService gptService)
        {
            _gptService = gptService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Prompt))
            {
                return BadRequest("Prompt-ul nu poate fi gol.");
            }

            var response = await _gptService.GetChatResponse(request.Prompt);
            return Ok(new { Response = response });
        }
    }

    public class ChatRequest
    {
        public string Prompt { get; set; }
    }
}
