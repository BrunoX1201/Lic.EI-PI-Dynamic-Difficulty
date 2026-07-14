using UnityEngine;

namespace Unity.FPS.DDA
{
    public static class DDAModifierState
    {
        public static TotalMobilesDDA TotalMobilesModifier { get; } = new(1);
        public static TotalTurretsDDA TotalTurretsModifier { get; } = new(0);

        public static MobileHealthDDA MobileHealthModifier { get; } = new(1f);
        public static MobileHitboxDDA MobileHitboxModifier { get; } = new(1f);

        public static TurretHealthDDA TurretHealthModifier { get; } = new(1f);
        public static TurretHitboxDDA TurretHitboxModifier { get; } = new(1f);

        public static bool HasReceivedOutput { get; private set; }
        public static DDAEncounterRestrictionsSO LastUsedRestrictions { get; private set; }

        public static void SetLastUsedRestrictions(DDAEncounterRestrictionsSO restrictions)
        {
            if (restrictions != null)
                LastUsedRestrictions = restrictions;
        }

        public static void Initialize()
        {
            DDAEventManager.AddListener<DDAModelOutputReceived>(OnModelOutputReceived);
        }

        private static void OnModelOutputReceived(DDAModelOutputReceived output)
        {
            TotalMobilesModifier.Update(output.TotalMobiles);
            TotalTurretsModifier.Update(output.TotalTurrets);
            MobileHealthModifier.Update(output.MobileHealth);
            MobileHitboxModifier.Update(output.MobileHitbox);
            TurretHealthModifier.Update(output.TurretHealth);
            TurretHitboxModifier.Update(output.TurretHitbox);
            HasReceivedOutput = true;

            Debug.Log($"[DDAModifierState] Modifiers updated (agent={output.Agent}) → " +
                      $"Mobiles={TotalMobilesModifier.Value}({TotalMobilesModifier.Direction}), " +
                      $"Bosses={TotalTurretsModifier.Value}({TotalTurretsModifier.Direction}), " +
                      $"MobileHealth={MobileHealthModifier.Value}, MobileHitbox={MobileHitboxModifier.Value}, " +
                      $"TurretHealth={TurretHealthModifier.Value}, TurretHitbox={TurretHitboxModifier.Value}");

            DDAEventManager.Broadcast(new ModifiersUpdated(TotalMobilesModifier.Value, TotalTurretsModifier.Value));
        }
    }
}