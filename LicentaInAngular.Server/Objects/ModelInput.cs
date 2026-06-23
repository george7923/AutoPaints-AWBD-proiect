using Microsoft.ML.Data;
namespace LicentaInAngular.Server.Objects
{
    public class ModelInput
    {
        [ColumnName("input")]
        [VectorType(128, 128, 3)]
        public float[] ImageData { get; set; }
    }
}
