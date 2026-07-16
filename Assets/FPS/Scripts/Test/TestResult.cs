namespace Unity.FPS.Test
{
    public class TestResult<T>
    {
        public string MetricName { get; private set; }
        public T MetricValue { get; private set; }
        public bool Success { get; private set; }

        public TestResult(string metricKey, T metricValue, bool success)
        {
            MetricName = metricKey;
            MetricValue = metricValue;
            Success = success;
        }
    }
}