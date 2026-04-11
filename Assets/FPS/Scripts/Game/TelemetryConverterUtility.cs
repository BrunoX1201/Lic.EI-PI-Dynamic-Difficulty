using System.Collections.Generic;
using Telemetry.Shared;

namespace Unity.FPS.Game
{
    public class TelemetryConverterUtility
    {
        private static readonly Dictionary<WeaponAttackType, AttackType> s_gameWeaponTypes = new()
        {
            { WeaponAttackType.Melee, AttackType.Melee },
            { WeaponAttackType.Ranged, AttackType.Ranged }
        };

        private static readonly Dictionary<string, Weapon> s_gameWeaponNames = new()
        {
            { "blaster", Weapon.Blaster },
            { "shotgun", Weapon.Shotgun },
            { "disc launcher", Weapon.Launcher }
        };

        public static AttackType ConvertToTelemetryAttackType(WeaponAttackType type)
        {
            return s_gameWeaponTypes.GetValueOrDefault(type, AttackType.Unknown);
        }

        public static Weapon ConvertToTelemetryWeapon(string name)
        {
            return s_gameWeaponNames.GetValueOrDefault(name.ToLower(), Weapon.Unknown);
        }
    }
}