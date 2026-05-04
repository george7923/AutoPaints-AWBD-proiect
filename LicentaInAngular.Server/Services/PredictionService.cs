using LicentaInAngular.Server.Interfaces;
using LicentaInAngular.Server.Objects;
using Microsoft.ML;
using Microsoft.ML.TensorFlow;
using System.Drawing;

namespace LicentaInAngular.Server.Services
{
    public class PredictionService : IPredictionService
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _mlModel;
        private readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;

        // Array de clase – folosit pentru maparea indexului la numele defectului
        private readonly string[] _defectClasses = new string[]
        {
            "crazing", "inclusion", "patches", "pitted_surface", "rolled_in_scale", "scratches"
        };

        public PredictionService()
        {
            try
            {
                _mlContext = new MLContext();

                // Calea catre modelul TensorFlow SavedModel sau modelul inghetat (.pb)
                string savedModelPath = @"C:\Users\George\Desktop\LICENTA\AI_Licenta\MODELELE\MetalDefectDetection_custom_frozen.pb";

                // Încarcă modelul TensorFlow folosind mlContext.Model
                var pipeline = _mlContext.Model.LoadTensorFlowModel(savedModelPath)
                    .ScoreTensorFlowModel(
                         outputColumnNames: new[] { "Identity" }, // Actualizeaza in functie de modelul tau (inspecteaza cu Netron)
                         inputColumnNames: new[] { "input" },       // Asigura-te ca numele corespund cu cele din modelul inghețat
                         addBatchDimensionInput: true
                    );

                // Creeaza un DataView gol pentru a "fit"-a pipeline-ul
                var emptyData = _mlContext.Data.LoadFromEnumerable(new List<ModelInput>());
                _mlModel = pipeline.Fit(emptyData);

                // Creeaza un PredictionEngine pentru predictii individuale
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(_mlModel);
            }
            catch (Exception ex)
            {
                // Afisează detaliile complete ale erorii la initializare
                Console.WriteLine("Eroare la inițializarea PredictionService: " + ex.ToString());
                throw;
            }
        }

        public ModelOutput Predict(ModelInput input)
        {
            try
            {
                return _predictionEngine.Predict(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eroare la Predict: " + ex.ToString());
                throw;
            }
        }

        public ModelOutput PredictFromFile(IFormFile imageFile)
        {
            float[] imageData = PreprocessImage(imageFile);
            if (imageData == null)
                throw new Exception("Preprocesarea imaginii a eșuat.");
            try
            {
                ModelInput input = new ModelInput { ImageData = imageData };
                return Predict(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eroare la PredictFromFile: " + ex.ToString());
                throw;
            }
        }

        public PredictionResult PredictWithLabel(IFormFile imageFile)
        {
            try
            {
                var output = PredictFromFile(imageFile);

                if (output == null)
                {
                    throw new Exception("ModelOutput returned null.");
                }

                if (output.PredictedLabels == null)
                {
                    throw new Exception("PredictedLabels is null. Asigurați-vă că modelul returnează un output valid.");
                }

                float maxProb = output.PredictedLabels.Max();
                int predictedIndex = output.PredictedLabels.ToList().IndexOf(maxProb);
                string predictedClass = _defectClasses[predictedIndex];

                return new PredictionResult
                {
                    DefectClass = predictedClass,
                    Probability = maxProb,
                    RawOutput = output.PredictedLabels
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eroare la PredictWithLabel: " + ex.ToString());
                throw;
            }
        }


        private float[] PreprocessImage(IFormFile imageFile)
        {
            try
            {
                using (var stream = imageFile.OpenReadStream())
                using (var bitmap = new Bitmap(stream))
                {
                    // Redimensioneaza imaginea la 128x128
                    var resizedBitmap = new Bitmap(bitmap, new Size(128, 128));
                    float[] imageData = new float[128 * 128 * 3];
                    int index = 0;
                    for (int y = 0; y < 128; y++)
                    {
                        for (int x = 0; x < 128; x++)
                        {
                            Color pixel = resizedBitmap.GetPixel(x, y);
                            // Normalizeaza pixelii de la 0-255 la 0-1
                            imageData[index++] = pixel.R / 255f;
                            imageData[index++] = pixel.G / 255f;
                            imageData[index++] = pixel.B / 255f;
                        }
                    }
                    return imageData;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Eroare în PreprocessImage: " + ex.ToString());
                return null;
            }
        }
    }
}
