namespace Unity.FPS.Telemetry
{
    public interface ITelemetryInstigator
    {
        public InstigatorType Type { get; }
        public int Id { get; }
        public ITelemetryMapLocation MapLocation { get; }
        public ITelemetryHealth Health { get; }
    }
}