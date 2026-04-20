namespace Unity.FPS.Telemetry
{
    public readonly struct GameTimePassedTelemetryData
    {
        public int TotalGameTimeSeconds { get; }
        public ITelemetryMapLocation PlayerLocation { get; }

        public GameTimePassedTelemetryData(int totalGameTimeSeconds, ITelemetryMapLocation playerLocation)
        {
            TotalGameTimeSeconds = totalGameTimeSeconds;
            PlayerLocation = playerLocation;
        }
    }

    public class GameTimePassedTelemetry : TelemetryEvent
    {
        public GameTimePassedTelemetry(int sessionId, GameTimePassedTelemetryData data) : base(sessionId,
            TelemetryEventType.GameTimePassed)
        {
            Data.Add("TotalGameTimeSeconds", data.TotalGameTimeSeconds);
            Data.Add("PlayerLocation", data.PlayerLocation?.Location);
        }
    }
}