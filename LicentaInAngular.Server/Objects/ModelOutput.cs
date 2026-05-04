using Microsoft.ML.Data;

namespace LicentaInAngular.Server.Objects
{
    public class ModelOutput
    {
        [ColumnName("Identity")]
        public float[] PredictedLabels { get; set; }
    }
}
