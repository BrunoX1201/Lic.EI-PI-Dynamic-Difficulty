namespace Unity.FPS.Telemetry
{
    public abstract class EncounterTelemetryData
    {
        public int Id { get; }

        protected EncounterTelemetryData(int id)
        {
            Id = id;
        }
    }

    public abstract class EncounterTelemetry : TelemetryEvent
    {
        protected EncounterTelemetry(string sessionId, TelemetryEventType eventType, EncounterTelemetryData data) :
            base(sessionId, eventType)
        {
            Data.Add("EncounterId", data.Id);
        }
    }
}