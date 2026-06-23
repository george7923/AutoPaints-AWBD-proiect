using Microsoft.AspNetCore.Mvc;
using LicentaInAngular.Server.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using LicentaInAngular.Server.Interfaces;

namespace LicentaInAngular.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PredictionController : ControllerBase
    {
        private readonly IPredictionService _predictionService;

        private readonly string[] _defectClasses = new string[]
        {
            "crazing", "inclusion", "patches", "pitted_surface", "rolled_in_scale", "scratches"
        };

        public PredictionController(IPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        /// <summary>
        /// Endpoint-ul care primește o imagine si returneaza predictia modelului.
        /// </summary>
        /// <param name="imageFile">Fisierul imagine incarcat prin formular</param>
        /// <returns>Un JSON cu clasa defectului detectat si probabilitatea aferenta</returns>
        [HttpPost("predict")]
        public IActionResult Predict([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Nu a fost furnizată nicio imagine.");
            }

            try
            {
                var prediction = _predictionService.PredictFromFile(imageFile);

                if (prediction == null || prediction.PredictedLabels == null)
                {
                    throw new Exception("ModelOutput sau PredictedLabels este null. Modelul nu a returnat niciun output.");
                }

                float maxProb = prediction.PredictedLabels.Max();
                int predictedIndex = prediction.PredictedLabels.ToList().IndexOf(maxProb);
                string predictedClass = _defectClasses[predictedIndex];
                Console.WriteLine("PREDICTIE!!!" + predictedClass);
                return Ok(new
                {
                    DefectClass = predictedClass,
                    Probability = maxProb,
                    RawOutput = prediction.PredictedLabels
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Eroare la predicție: " + ex.ToString());
            }
        }


    }
}
