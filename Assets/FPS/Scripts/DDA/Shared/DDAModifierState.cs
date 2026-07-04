using UnityEngine;

namespace Unity.FPS.DDA
{
    public static class DDAModifierState
    {
        public static TotalEnemiesDDA TotalEnemiesModifier { get; } = new(1);
        public static EnemyHealthDDA EnemyHealthModifier { get; } = new(1f);
        public static EnemyHitboxDDA EnemyHitboxModifier { get; } = new(1f);
        public static bool HasReceivedOutput { get; private set; }

        public static void Initialize()
        {
            DDAEventManager.AddListener<DDAModelOutputReceived>(OnModelOutputReceived);
        }

        private static void OnModelOutputReceived(DDAModelOutputReceived output)
        {
            TotalEnemiesModifier.Update(output.TotalEnemies, output.Agent);
            EnemyHealthModifier.Update(output.EnemyHealth, output.Agent);
            EnemyHitboxModifier.Update(output.EnemyHitbox, output.Agent);
            HasReceivedOutput = true;

            Debug.Log($"[DDAModifierState] Modifiers updated (agent={output.Agent}) → " +
                      $"TotalEnemies={TotalEnemiesModifier.Value} ({TotalEnemiesModifier.Direction}), " +
                      $"EnemyHealth={EnemyHealthModifier.Value} ({EnemyHealthModifier.Direction}), " +
                      $"EnemyHitbox={EnemyHitboxModifier.Value} ({EnemyHitboxModifier.Direction})");

            DDAEventManager.Broadcast(new ModifiersUpdated(
                TotalEnemiesModifier.Value, EnemyHealthModifier.Value, EnemyHitboxModifier.Value));
        }

        // --- Usado pelo custom editor/debug para ajustar modificadores individualmente em runtime ---

        public static void SetTotalEnemies(int value, string agent)
        {
            TotalEnemiesModifier.Update(value, agent);
            DDAEventManager.Broadcast(new TotalEnemiesModifierChanged(value));
        }

        public static void SetEnemyHealth(float value, string agent)
        {
            EnemyHealthModifier.Update(value, agent);
            DDAEventManager.Broadcast(new EnemyHealthModifierChanged(value));
        }

        public static void SetEnemyHitbox(float value, string agent)
        {
            EnemyHitboxModifier.Update(value, agent);
            DDAEventManager.Broadcast(new EnemyHitboxModifierChanged(value));
        }
    }
}