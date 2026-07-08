namespace Unity.FPS.DDA
{
    /// <summary>Published by DDAService when an encounter completes.</summary>
    public class EncounterCompleted : DDAEvent
    {
        public string EncounterId { get; }

        public EncounterCompleted(string encounterId)
        {
            EncounterId = encounterId;
        }
    }

    /// <summary>Published by DDAController with the raw model output.</summary>
    public class DDAModelOutputReceived : DDAEvent
    {
        public int TotalMobiles { get; }
        public int TotalTurrets { get; }
        public float MobileHealth { get; }
        public float MobileHitbox { get; }
        public float TurretHealth { get; }
        public float TurretHitbox { get; }
        public string Agent { get; }
        public string Action { get; }

        public DDAModelOutputReceived(int totalMobiles, int totalTurrets,
            float mobileHealth, float mobileHitbox, float turretHealth, float turretHitbox, string agent, string action)
        {
            TotalMobiles = totalMobiles;
            TotalTurrets = totalTurrets;
            MobileHealth = mobileHealth;
            MobileHitbox = mobileHitbox;
            TurretHealth = turretHealth;
            TurretHitbox = turretHitbox;
            Agent = agent;
            Action = action;
        }
    }

    public class ModifiersUpdated : DDAEvent
    {
        public int TotalMobiles { get; }
        public int TotalBosses { get; }

        public ModifiersUpdated(int totalMobiles, int totalBosses)
        {
            TotalMobiles = totalMobiles;
            TotalBosses = totalBosses;
        }
    }

    /// <summary>Published when only TotalEnemies muda individualmente (ex: custom editor).</summary>
    public class TotalEnemiesModifierChanged : DDAEvent
    {
        public int TotalEnemies { get; }

        public TotalEnemiesModifierChanged(int totalEnemies)
        {
            TotalEnemies = totalEnemies;
        }
    }

    /// <summary>Published when only EnemyHealth muda individualmente (ex: custom editor).</summary>
    public class EnemyHealthModifierChanged : DDAEvent
    {
        public float EnemyHealth { get; }

        public EnemyHealthModifierChanged(float enemyHealth)
        {
            EnemyHealth = enemyHealth;
        }
    }

    /// <summary>Published when only EnemyHitbox muda individualmente (ex: custom editor).</summary>
    public class EnemyHitboxModifierChanged : DDAEvent
    {
        public float EnemyHitbox { get; }

        public EnemyHitboxModifierChanged(float enemyHitbox)
        {
            EnemyHitbox = enemyHitbox;
        }
    }

    public class EncounterEnemiesSpawnedEvent : DDAEvent
    {
        public string EncounterId { get; }

        public EncounterEnemiesSpawnedEvent(string encounterId)
        {
            EncounterId = encounterId;
        }
    }

    /// <summary>Published by DDAController quando o pedido à API falha (erro de rede ou resposta inválida).</summary>
    public class DDAPredictionFailed : DDAEvent
    {
        public string EncounterId { get; }

        public DDAPredictionFailed(string encounterId)
        {
            EncounterId = encounterId;
        }
    }
}