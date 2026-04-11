namespace Unity.FPS.Telemetry
{
    public interface ITelemetryMapLocation
    {
        public int GameProgress { get; }
        public string Location { get; }
    }
}