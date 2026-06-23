namespace LicentaInAngular.Server.Objects
{
    public class PredictionResult
    {
        public string DefectClass { get; set; }
        public float Probability { get; set; }
        public float[] RawOutput { get; set; }
    }
}
