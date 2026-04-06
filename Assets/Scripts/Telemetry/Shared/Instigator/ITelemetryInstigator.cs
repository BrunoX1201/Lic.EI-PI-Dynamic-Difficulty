namespace Telemetry.Shared.Instigator
{
    public interface ITelemetryInstigator
    {
        public InstigatorType Type { get; }
        public int Id { get; }
        public ITelemetryMapLocation MapLocation { get; }
        public ITelemetryHealth Health { get; }
    }
}