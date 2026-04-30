using UnityEngine;

namespace Unity.FPS.Telemetry
{
    public readonly struct PlayerWentAirborneTelemetryData
    {
        public Vector3 StartPosition { get; }
        public Vector3 EndPosition { get; }
        public AirborneType AirborneType { get; }

        public PlayerWentAirborneTelemetryData(Vector3 startPosition, Vector3 endPosition, AirborneType airborneType)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            AirborneType = airborneType;
        }
    }

    public class PlayerWentAirborneTelemetry : TelemetryEvent
    {
        public PlayerWentAirborneTelemetry(string sessionId, PlayerWentAirborneTelemetryData data) : base(sessionId,
            TelemetryEventType.PlayerWentAirborne)
        {
            Data.Add("StartPosition", data.StartPosition);
            Data.Add("EndPosition", data.EndPosition);
            Data.Add("Action", data.AirborneType);
        }
    }
}