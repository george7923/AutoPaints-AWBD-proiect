using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LicentaInAngular.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaintAnalysisController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PaintAnalysisController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Endpoint-ul care primește un fisier imagine si apeleaza API-ul Python pentru analiza culorii si detectarea metalizarii.
        /// </summary>
        /// <param name="imageFile">Imaginea trimisa din formularul Angular</param>
        /// <returns>JSON cu rezultatul analizei</returns>
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzePaint([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("Nu a fost furnizată nicio imagine.");

            // Creeaza un HttpClient folosind fabrica inregistrata (clientul "PaintAnalysisClient")
            var client = _httpClientFactory.CreateClient("PaintAnalysisClient");

            // Pregateste continutul cererii (MultipartFormDataContent)
            using (var content = new MultipartFormDataContent())
            {
                var streamContent = new StreamContent(imageFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                content.Add(streamContent, "image", imageFile.FileName);

                // Apeleaza endpoint-ul din API-ul Python
                HttpResponseMessage response = await client.PostAsync("analyze-paint", content);
                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, "Eroare la analiza culorii vopselei.");

                var jsonResult = await response.Content.ReadAsStringAsync();
                return Ok(jsonResult);
            }
        }
    }
}
