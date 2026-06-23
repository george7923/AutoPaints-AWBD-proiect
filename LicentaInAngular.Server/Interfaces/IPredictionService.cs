using LicentaInAngular.Server.Objects;

namespace LicentaInAngular.Server.Interfaces
{
    public interface IPredictionService
    {
        /// <summary>
        /// Efectuează predicția pe baza unui input preprocesat.
        /// </summary>
        /// <param name="input">Obiectul de intrare (imaginea preprocesată ca vector)</param>
        /// <returns>Obiectul de ieșire cu predicțiile modelului</returns>
        ModelOutput Predict(ModelInput input);

        /// <summary>
        /// Efectuează predicția pe baza unui fișier imagine.
        /// </summary>
        /// <param name="imageFile">Fișierul imagine încărcat (IFormFile)</param>
        /// <returns>Obiectul de ieșire cu predicțiile modelului</returns>
        ModelOutput PredictFromFile(IFormFile imageFile);
        PredictionResult PredictWithLabel(IFormFile imageFile);
    }
}
