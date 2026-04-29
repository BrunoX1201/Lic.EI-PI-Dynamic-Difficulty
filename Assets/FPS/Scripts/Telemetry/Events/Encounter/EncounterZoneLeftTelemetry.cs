using UnityEngine;

namespace Unity.FPS.Telemetry
{
    public readonly struct EncounterZoneLeftTelemetryData
    {
        public Vector3 PlayerPosition { get; }

        public EncounterZoneLeftTelemetryData(Vector3 playerPosition)
        {
            PlayerPosition = playerPosition;
        }
    }

    public class EncounterZoneLeftTelemetry : TelemetryEvent
    {
        public EncounterZoneLeftTelemetry(string sessionId, EncounterZoneLeftTelemetryData data) : base(sessionId,
            TelemetryEventType.EncounterZoneLeft)
        {
            Data.Add("PlayerPosition", data.PlayerPosition);
        }
    }
}