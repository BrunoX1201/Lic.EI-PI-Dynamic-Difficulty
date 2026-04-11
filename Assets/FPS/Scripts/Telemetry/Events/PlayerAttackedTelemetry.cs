using Telemetry.Shared;
using UnityEngine;

namespace Unity.FPS.Telemetry
{
    public readonly struct PlayerAttackedTelemetryData
    {
        public int PlayerId { get; }
        public int TargetId { get; }
        public bool IsHit { get; }
        public float Accuracy { get; }
        public Vector3 PlayerPosition { get; }
        public Weapon WeaponUsed { get; }
        public float DamagePerHit { get; }
        public AttackType AttackType { get; }
        public float RemainingAmmo { get; }

        public PlayerAttackedTelemetryData(
            int playerId,
            int targetId,
            bool isHit,
            float accuracy,
            Vector3 playerPosition,
            Weapon weaponUsed,
            float damagePerHit,
            AttackType attackType,
            float remainingAmmo)
        {
            PlayerId = playerId;
            TargetId = targetId;
            IsHit = isHit;
            Accuracy = accuracy;
            PlayerPosition = playerPosition;
            WeaponUsed = weaponUsed;
            DamagePerHit = damagePerHit;
            AttackType = attackType;
            RemainingAmmo = remainingAmmo;
        }
    }

    public class PlayerAttackedTelemetry : TelemetryEvent
    {
        public PlayerAttackedTelemetry(int sessionId, PlayerAttackedTelemetryData data)
            : base(sessionId, TelemetryEventType.PlayerAttacked)
        {
            Data.Add("PlayerId", data.PlayerId);
            Data.Add("TargetId", data.TargetId);
            Data.Add("IsHit", data.IsHit);
            Data.Add("Accuracy", data.Accuracy);
            Data.Add("PlayerPosition", data.PlayerPosition);
            Data.Add("WeaponUsed", data.WeaponUsed.ToString());
            Data.Add("DamagePerHit", data.DamagePerHit);
            Data.Add("AttackType", data.AttackType);
            Data.Add("RemainingAmmo", data.RemainingAmmo);
        }
    }
}