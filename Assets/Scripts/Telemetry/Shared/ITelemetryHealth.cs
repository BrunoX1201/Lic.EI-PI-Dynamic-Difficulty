namespace Telemetry.Shared
{
    public interface ITelemetryHealth
    {
        public float Health { get; }
        public float MaxHealth { get; }
    }
}