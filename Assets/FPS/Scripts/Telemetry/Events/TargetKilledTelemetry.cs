using Telemetry.Shared;
using UnityEngine;

namespace Unity.FPS.Telemetry
{
    public readonly struct TargetKilledTelemetryData
    {
        public int TargetId { get; }
        public Weapon WeaponUsed { get; }
        public float RemainingAmmo { get; }
        public float RemainingHealth { get; }
        public Vector3 PlayerPosition { get; }
        public ITelemetryMapLocation PlayerLocation { get; }

        public TargetKilledTelemetryData(int targetId, Weapon weaponUsed, float remainingAmmo, float remainingHealth,
            Vector3 playerPosition, ITelemetryMapLocation playerLocation)
        {
            TargetId = targetId;
            WeaponUsed = weaponUsed;
            RemainingAmmo = remainingAmmo;
            RemainingHealth = remainingHealth;
            PlayerPosition = playerPosition;
            PlayerLocation = playerLocation;
        }
    }

    public class TargetKilledTelemetry : TelemetryEvent
    {
        public TargetKilledTelemetry(string sessionId, TargetKilledTelemetryData data) : base(sessionId,
            TelemetryEventType.TargetKilled)
        {
            Data.Add("TargetId", data.TargetId);
            Data.Add("WeaponUsed", data.WeaponUsed);
            Data.Add("RemainingAmmo", data.RemainingAmmo);
            Data.Add("RemainingHealth", data.RemainingHealth);
            Data.Add("PlayerPosition", data.PlayerPosition);
            Data.Add("PlayerLocation", data.PlayerLocation?.Location);
        }
    }
}