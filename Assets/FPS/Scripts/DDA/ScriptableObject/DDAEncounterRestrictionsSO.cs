using UnityEngine;

namespace Unity.FPS.DDA
{
    [CreateAssetMenu(fileName = "DDAEncounterRestrictionsSO", menuName = "Scriptable Objects/DDAEncounterRestrictionsSO")]
    public class DDAEncounterRestrictionsSO : ScriptableObject
    {
        [Header("Mobiles (standard)")]
        public int DefaultMobiles = 3;
        public int MinMobiles = 1;
        public int MaxMobiles = 10;

        [Header("Bosses (turret)")]
        public int DefaultBosses = 0;
        public int MinBosses = 0;
        public int MaxBosses = 0;

        [Header("Vida/Hitbox Mobile")]
        public float DefaultMobileHealth = 100f;
        public float MinMobileHealth = 50f;
        public float MaxMobileHealth = 300f;
        public float DefaultMobileHitbox = 1f;
        public float MinMobileHitbox = 0.5f;
        public float MaxMobileHitbox = 2f;

        [Header("Vida/Hitbox Turret")]
        public float DefaultTurretHealth = 150f;
        public float MinTurretHealth = 80f;
        public float MaxTurretHealth = 400f;
        public float DefaultTurretHitbox = 1f;
        public float MinTurretHitbox = 0.5f;
        public float MaxTurretHitbox = 2f;
    }
}