namespace Unity.FPS.Telemetry
{
    public enum EncounterEndReason
    {
        Completed = 0,
        PlayerDied = 1
    }

    public class EncounterEndedTelemetryData : EncounterTelemetryData
    {
        public EncounterEndReason EndReason { get; }

        public EncounterEndedTelemetryData(int id, EncounterEndReason endReason) : base(id)
        {
            EndReason = endReason;
        }
    }

    public class EncounterEndedTelemetry : EncounterTelemetry
    {
        public EncounterEndedTelemetry(string sessionId, EncounterEndedTelemetryData data) : base(sessionId,
            TelemetryEventType.EncounterEnded, data)
        {
            Data.Add("EndReason", data.EndReason);
        }
    }
}