using UnityEngine;

namespace Unity.FPS.Telemetry
{
    public readonly struct PlayerTookDamageTelemetryData
    {
        public float DamageTakenPerHit { get; }
        public float PlayerMoveSpeed { get; }
        public ITelemetryInstigator Instigator { get; }
        public float PlayerCurrentHealth { get; }
        public Vector3 PlayerPosition { get; }

        public PlayerTookDamageTelemetryData(float damageTakenPerHit, float playerMoveSpeed,
            ITelemetryInstigator instigator, float playerCurrentHealth, Vector3 playerPosition)
        {
            DamageTakenPerHit = damageTakenPerHit;
            PlayerMoveSpeed = playerMoveSpeed;
            Instigator = instigator;
            PlayerCurrentHealth = playerCurrentHealth;
            PlayerPosition = playerPosition;
        }
    }

    public class PlayerTookDamageTelemetry : TelemetryEvent
    {
        public PlayerTookDamageTelemetry(int sessionId, PlayerTookDamageTelemetryData data) : base(sessionId,
            TelemetryEventType.PlayerTookDamage)
        {
            Data.Add("DamageTakenPerHit", data.DamageTakenPerHit);
            Data.Add("PlayerMoveSpeed", data.PlayerMoveSpeed);
            Data.Add("Instigator", data.Instigator);
            Data.Add("PlayerCurrentHealth", data.PlayerCurrentHealth);
            Data.Add("PlayerPosition", data.PlayerPosition);
        }
    }
}