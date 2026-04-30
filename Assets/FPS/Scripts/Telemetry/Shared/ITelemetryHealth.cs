namespace Unity.FPS.Telemetry
{
    public interface ITelemetryHealth
    {
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
    }
}