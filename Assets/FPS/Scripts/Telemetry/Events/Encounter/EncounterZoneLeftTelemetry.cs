using UnityEngine;

namespace Unity.FPS.Telemetry
{
    public class EncounterZoneLeftTelemetryData : EncounterTelemetryData
    {
        public Vector3 PlayerPosition { get; }

        public EncounterZoneLeftTelemetryData(string id, Vector3 playerPosition) : base(id)
        {
            PlayerPosition = playerPosition;
        }
    }

    public class EncounterZoneLeftTelemetry : EncounterTelemetry
    {
        public EncounterZoneLeftTelemetry(string sessionId, EncounterZoneLeftTelemetryData data) : base(sessionId,
            TelemetryEventType.EncounterZoneLeft, data)
        {
            Data.Add("PlayerPosition", data.PlayerPosition);
        }
    }
}