namespace Unity.FPS.Telemetry
{
    public enum EncounterStartReason
    {
        Entered = 0,
        AttackedEnemy = 1
    }

    public class EncounterStartedTelemetryData : EncounterTelemetryData
    {
        public EncounterStartReason StartReason { get; }

        public EncounterStartedTelemetryData(int id, EncounterStartReason startReason) : base(id)
        {
            StartReason = startReason;
        }
    }

    public class EncounterStartedTelemetry : EncounterTelemetry
    {
        public EncounterStartedTelemetry(string sessionId, EncounterStartedTelemetryData data) : base(sessionId,
            TelemetryEventType.EncounterStarted, data)
        {
            Data.Add("StartReason", data.StartReason);
        }
    }
}