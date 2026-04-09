using System;
using UnityEngine;

namespace Unity.FPS.Telemetry
{
    [Serializable]
    public readonly struct PlayerDiedTelemetryData
    {
        public ITelemetryInstigator Instigator { get; }
        public int DeathCount { get; }
        public int TimeAliveSeconds { get; }
        public Vector3 PlayerPosition { get; }

        public PlayerDiedTelemetryData(ITelemetryInstigator instigator, int deathCount, int timeAliveSeconds,
            Vector3 playerPosition)
        {
            Instigator = instigator;
            DeathCount = deathCount;
            TimeAliveSeconds = timeAliveSeconds;
            PlayerPosition = playerPosition;
        }
    }

    public class PlayerDiedTelemetry : TelemetryEvent
    {
        public PlayerDiedTelemetry(int sessionId, PlayerDiedTelemetryData data) : base(sessionId,
            TelemetryEventType.PlayerDied)
        {
            Data.Add("KilledBy", data.Instigator.Id);
            Data.Add("DeathCount", data.DeathCount);
            Data.Add("TimeAliveSeconds", data.TimeAliveSeconds);
            Data.Add("PlayerPosition", data.PlayerPosition);
        }
    }
}