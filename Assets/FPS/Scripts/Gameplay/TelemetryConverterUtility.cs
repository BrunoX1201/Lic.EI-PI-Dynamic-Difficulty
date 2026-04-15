using System.Collections.Generic;
using FPS.Scripts.Telemetry.Shared;
using Telemetry.Shared;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public static class TelemetryConverterUtility
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

        private static readonly Dictionary<PickupType, Item> s_gamePickupTypes = new()
        {
            { PickupType.Health, Item.Health },
            { PickupType.Ammo, Item.Ammo },
            { PickupType.Jetpack, Item.Jetpack },
            { PickupType.Weapon, Item.Weapon }
        };

        public static AttackType ConvertToTelemetryAttackType(WeaponAttackType type)
        {
            return s_gameWeaponTypes.GetValueOrDefault(type, AttackType.Unknown);
        }

        public static Weapon ConvertToTelemetryWeapon(string name)
        {
            return s_gameWeaponNames.GetValueOrDefault(name.ToLower(), Weapon.Unknown);
        }

        public static Item ConvertToTelemetryPickupType(PickupType type)
        {
            return s_gamePickupTypes.GetValueOrDefault(type, Item.Unknown);
        }
    }
}