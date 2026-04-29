using UnityEngine;

namespace Unity.FPS.Telemetry
{
    public class EncounterZoneEnteredTelemetryData : EncounterTelemetryData
    {
        public Vector3 PlayerPosition { get; }

        public EncounterZoneEnteredTelemetryData(int id, Vector3 playerPosition) : base(id)
        {
            PlayerPosition = playerPosition;
        }
    }

    public class EncounterZoneEnteredTelemetry : EncounterTelemetry
    {
        public EncounterZoneEnteredTelemetry(string sessionId, EncounterZoneEnteredTelemetryData data) : base(sessionId,
            TelemetryEventType.EncounterZoneEntered, data)
        {
            Data.Add("PlayerPosition", data.PlayerPosition);
        }
    }
}