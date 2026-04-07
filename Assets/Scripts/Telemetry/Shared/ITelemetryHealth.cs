namespace Telemetry.Shared
{
    public interface ITelemetryHealth
    {
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
    }
}