using UnityEngine;

namespace Unity.FPS.Telemetry
{
    public readonly struct EncounterZoneEnteredTelemetryData
    {
        public Vector3 PlayerPosition { get; }

        public EncounterZoneEnteredTelemetryData(Vector3 playerPosition)
        {
            PlayerPosition = playerPosition;
        }
    }

    public class EncounterZoneEnteredTelemetry : TelemetryEvent
    {
        public EncounterZoneEnteredTelemetry(string sessionId, EncounterZoneEnteredTelemetryData data) : base(sessionId,
            TelemetryEventType.EncounterZoneEntered)
        {
            Data.Add("PlayerPosition", data.PlayerPosition);
        }
    }
}