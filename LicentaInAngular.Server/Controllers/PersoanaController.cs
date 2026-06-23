using Microsoft.AspNetCore.Mvc;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersoanaController : ControllerBase
    {
        private readonly PersoanaService _persoanaService;

        public PersoanaController(PersoanaService persoanaService)
        {
            _persoanaService = persoanaService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return await _persoanaService.GetByIdResponse(id);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            return await _persoanaService.GetByEmailResponse(email);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Persoana persoana)
        {
            return await _persoanaService.CreateResponse(persoana);
        }

        [HttpPut("updateById/{id}")]
        public async Task<IActionResult> UpdatePersoanaById(int id, [FromBody] Persoana updatedPersoana)
        {
            return await _persoanaService.UpdatePersoanaByIdResponse(id, updatedPersoana);
        }

        [HttpDelete("deleteById/{id}")]
        public async Task<IActionResult> DeletePersoanaById(int id)
        {
            return await _persoanaService.DeletePersoanaByIdResponse(id);
        }

        [HttpPost("send-email")]
        public IActionResult SendEmail([FromBody] EmailRequest request)
        {
            return _persoanaService.SendEmailResponse(request);
        }

        public class EmailRequest
        {
            public string To { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }
    }
}