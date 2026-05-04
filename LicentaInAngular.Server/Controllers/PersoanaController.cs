using Microsoft.AspNetCore.Mvc;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersoanaController : ControllerBase
    {
        private readonly IPersoanaRepository _persoanaRepository;

        public PersoanaController(IPersoanaRepository persoanaRepository)
        {
            _persoanaRepository = persoanaRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var persoana = await _persoanaRepository.GetById(id);
            if (persoana == null)
                return NotFound();
            return Ok(persoana);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var persoana = await _persoanaRepository.GetByEmail(email);
            if (persoana == null)
                return NotFound();
            return Ok(persoana);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Persoana persoana)
        {
            await _persoanaRepository.CreatePersoana(persoana);
            return CreatedAtAction(nameof(GetById), new { id = persoana.IdPersoana }, persoana);
        }

        [HttpPut("updateById/{id}")]
        public async Task<IActionResult> UpdatePersoanaById(int id, [FromBody] Persoana updatedPersoana)
        {
            if (updatedPersoana == null)
                return BadRequest(new { error = "Invalid Persoana data" });
            await _persoanaRepository.UpdatePersoanaById(id, updatedPersoana);
            return Ok(new { message = "Persoana actualizata cu succes" });
        }

        [HttpDelete("deleteById/{id}")]
        public async Task<IActionResult> DeletePersoanaById(int id)
        {
            var persoana = await _persoanaRepository.GetById(id);
            if (persoana == null)
                return NotFound(new { message = "Persoana not found" });
            await _persoanaRepository.DeletePersoanaById(id);
            return Ok(new { message = "Persoana deleted successfully" });
        }
        [HttpPost("send-email")]
        public IActionResult SendEmail([FromBody] EmailRequest request)
        {
            try
            {
                var fromAddress = new MailAddress("kobrageorge792@gmail.com", "AutoPaints Team");
                var toAddress = new MailAddress(request.To);
                const string fromPassword = "oarl ezrd ecti fmuo";  // Gmail app password
                string subject = request.Subject;
                string body = request.Body;

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }

                return Ok(new { message = "Email trimis cu succes!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Eroare la trimiterea emailului", details = ex.Message });
            }
        }
        public class EmailRequest
        {
            public string To { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }
    }
}
