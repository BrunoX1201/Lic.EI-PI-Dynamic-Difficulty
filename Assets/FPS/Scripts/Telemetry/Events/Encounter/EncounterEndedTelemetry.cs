namespace Unity.FPS.Telemetry
{
    public enum EncounterEndReason
    {
        Completed = 0,
        PlayerDied = 1
    }

    public readonly struct EncounterEndedTelemetryData
    {
        public EncounterEndReason EndReason { get; }

        public EncounterEndedTelemetryData(EncounterEndReason endReason)
        {
            EndReason = endReason;
        }
    }

    public class EncounterEndedTelemetry : TelemetryEvent
    {
        public EncounterEndedTelemetry(string sessionId, EncounterEndedTelemetryData data) : base(sessionId,
            TelemetryEventType.EncounterEnded)
        {
            Data.Add("EndReason", data.EndReason);
        }
    }
}