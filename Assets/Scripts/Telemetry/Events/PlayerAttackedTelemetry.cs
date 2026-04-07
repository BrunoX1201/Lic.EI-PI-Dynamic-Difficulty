using Telemetry.Enums;
using UnityEngine;

namespace Telemetry.Events
{
    public class PlayerAttackedTelemetry : TelemetryEvent
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

        // O override da propriedade Data da interface/classe base

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
            PlayerId = playerId;
            TargetId = targetId;
            IsHit = isHit;
            Accuracy = accuracy;
            PlayerPosition = playerPosition;
            WeaponUsed = weaponUsed;
            DamagePerHit = damagePerHit;
            AttackType = attackType;
            RemainingAmmo = remainingAmmo;
            {
                Data.Add("PlayerId", PlayerId);
                Data.Add("TargetId", TargetId);
                Data.Add("IsHit", IsHit);
                Data.Add("Accuracy", Accuracy);
                Data.Add("PlayerPosition", PlayerPosition);
                Data.Add("WeaponUsed", WeaponUsed.ToString());
                Data.Add("DamagePerHit", DamagePerHit);
                Data.Add("AttackType", AttackType);
                Data.Add("RemainingAmmo", RemainingAmmo);
            }
            ;
        }
    }
}