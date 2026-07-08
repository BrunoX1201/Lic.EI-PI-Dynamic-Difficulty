namespace Unity.FPS.DDA
{
    [System.Serializable]
    public struct DDAEncounterRestrictions
    {
        public int DefaultMobiles;
        public int MinMobiles;
        public int MaxMobiles;

        public int DefaultBosses;
        public int MinBosses;
        public int MaxBosses;

        public float DefaultMobileHealth;
        public float MinMobileHealth;
        public float MaxMobileHealth;

        public float DefaultMobileHitbox;
        public float MinMobileHitbox;
        public float MaxMobileHitbox;

        public float DefaultTurretHealth;
        public float MinTurretHealth;
        public float MaxTurretHealth;

        public float DefaultTurretHitbox;
        public float MinTurretHitbox;
        public float MaxTurretHitbox;

        public DDAEncounterRestrictions(
            int defaultMobiles, int minMobiles, int maxMobiles,
            int defaultBosses, int minBosses, int maxBosses,
            float defaultMobileHealth, float minMobileHealth, float maxMobileHealth,
            float defaultMobileHitbox, float minMobileHitbox, float maxMobileHitbox,
            float defaultTurretHealth, float minTurretHealth, float maxTurretHealth,
            float defaultTurretHitbox, float minTurretHitbox, float maxTurretHitbox)
        {
            DefaultMobiles = defaultMobiles;
            MinMobiles = minMobiles;
            MaxMobiles = maxMobiles;
            DefaultBosses = defaultBosses;
            MinBosses = minBosses;
            MaxBosses = maxBosses;
            DefaultMobileHealth = defaultMobileHealth;
            MinMobileHealth = minMobileHealth;
            MaxMobileHealth = maxMobileHealth;
            DefaultMobileHitbox = defaultMobileHitbox;
            MinMobileHitbox = minMobileHitbox;
            MaxMobileHitbox = maxMobileHitbox;
            DefaultTurretHealth = defaultTurretHealth;
            MinTurretHealth = minTurretHealth;
            MaxTurretHealth = maxTurretHealth;
            DefaultTurretHitbox = defaultTurretHitbox;
            MinTurretHitbox = minTurretHitbox;
            MaxTurretHitbox = maxTurretHitbox;
        }

        /// <summary>Usado só quando a próxima zona ainda não está disponível (ex: último encontro de um nível).</summary> /TODO: APAGAR NÃO NECESSARIO
        public static DDAEncounterRestrictions GlobalFallback => new(
            1, 1, 10,
            0, 0, 2,
            1f, 1f, 300f,
            0.1f, 0.1f, 2f,
            1f, 1f, 400f,
            0.1f, 0.1f, 2f);
    }
}