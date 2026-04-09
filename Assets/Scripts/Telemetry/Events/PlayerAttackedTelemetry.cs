using Telemetry.Shared;
using UnityEngine;

namespace Telemetry.Events
{
    public class PlayerAttackedTelemetry : TelemetryEvent
    {
        public PlayerAttackedTelemetry(
            int sessionId,
            int playerId,
            int targetId,
            bool isHit,
            float accuracy,
            Vector3 playerPosition,
            Weapon weaponUsed,
            float damagePerHit,
            AttackType attackType,
            float remainingAmmo)
            : base(sessionId, TelemetryEventType.PlayerAttacked)
        {
            Data.Add("PlayerId", playerId);
            Data.Add("TargetId", targetId);
            Data.Add("IsHit", isHit);
            Data.Add("Accuracy", accuracy);
            Data.Add("PlayerPosition", playerPosition);
            Data.Add("WeaponUsed", weaponUsed.ToString());
            Data.Add("DamagePerHit", damagePerHit);
            Data.Add("AttackType", attackType);
            Data.Add("RemainingAmmo", remainingAmmo);
        }
    }
}