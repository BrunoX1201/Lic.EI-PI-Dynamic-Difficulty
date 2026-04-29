namespace Unity.FPS.Telemetry
{
    public enum EncounterStartReason
    {
        Entered = 0,
        AttackedEnemy = 1
    }

    public readonly struct EncounterStartedTelemetryData
    {
        public EncounterStartReason StartReason { get; }

        public EncounterStartedTelemetryData(EncounterStartReason startReason)
        {
            StartReason = startReason;
        }
    }

    public class EncounterStartedTelemetry : TelemetryEvent
    {
        public EncounterStartedTelemetry(string sessionId, EncounterStartedTelemetryData data) : base(sessionId,
            TelemetryEventType.EncounterStarted)
        {
            Data.Add("StartReason", data.StartReason);
        }
    }
}