namespace Telemetry.Shared
{
    public interface ITelemetryMapLocation
    {
        public int GameProgress { get; }
        public string Location { get; }
    }
}