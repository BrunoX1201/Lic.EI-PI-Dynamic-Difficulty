namespace Unity.FPS.Telemetry
{
    public readonly struct NewLocationDiscoveredTelemetryData
    {
        public ITelemetryMapLocation LastLocation { get; }
        public ITelemetryMapLocation NewLocation { get; }
        public int TimeInLastLocationSeconds { get; }
        public bool HasDiscovered { get; }

        public NewLocationDiscoveredTelemetryData(ITelemetryMapLocation lastLocation, ITelemetryMapLocation newLocation,
            int timeInLastLocationSeconds, bool hasDiscovered)
        {
            LastLocation = lastLocation;
            NewLocation = newLocation;
            TimeInLastLocationSeconds = timeInLastLocationSeconds;
            HasDiscovered = hasDiscovered;
        }
    }

    public class NewLocationDiscoveredTelemetry : TelemetryEvent
    {
        public NewLocationDiscoveredTelemetry(string sessionId, NewLocationDiscoveredTelemetryData data)
            : base(sessionId, TelemetryEventType.NewLocationDiscovered)
        {
            Data.Add("LastLocation", data.LastLocation?.Location);
            Data.Add("NewLocation", data.NewLocation?.Location);
            Data.Add("TimeInLastLocationSeconds", data.TimeInLastLocationSeconds);
            Data.Add("HasDiscovered", data.HasDiscovered);
        }
    }
}