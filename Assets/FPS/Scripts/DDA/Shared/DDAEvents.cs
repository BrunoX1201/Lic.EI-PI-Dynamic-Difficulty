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
        public int TotalEnemies { get; }
        public float EnemyHealth { get; }
        public float EnemyHitbox { get; }
        public string Agent { get; }
        public string Action { get; }

        public DDAModelOutputReceived(int totalEnemies, float enemyHealth, float enemyHitbox, string agent,
            string action)
        {
            TotalEnemies = totalEnemies;
            EnemyHealth = enemyHealth;
            EnemyHitbox = enemyHitbox;
            Agent = agent;
            Action = action;
        }
    }

    /// <summary>
    /// Published by DDAModifierState sempre que o estado completo dos modificadores muda.
    /// EncounterZoneManager escuta isto para saber quando aplicar a próxima previsão.
    /// </summary>
    public class ModifiersUpdated : DDAEvent
    {
        public int TotalEnemies { get; }
        public float EnemyHealth { get; }
        public float EnemyHitbox { get; }

        public ModifiersUpdated(int totalEnemies, float enemyHealth, float enemyHitbox)
        {
            TotalEnemies = totalEnemies;
            EnemyHealth = enemyHealth;
            EnemyHitbox = enemyHitbox;
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