namespace Unity.FPS.Telemetry
{
    public enum EncounterStartReason
    {
        Entered = 0,
        AttackedEnemy = 1
    }

    public class EncounterStartedTelemetryData : EncounterTelemetryData
    {
        public EncounterStartReason StartReason { get; }
        public int TotalEnemies { get; }
        public float PlayerStartHealth { get; }
        public float PlayerStartAmmo { get; }
        public ITelemetryMapLocation Location { get; }

        public EncounterStartedTelemetryData(string id, EncounterStartReason startReason, int totalEnemies,
            float playerStartHealth, float playerStartAmmo, ITelemetryMapLocation location) : base(id)
        {
            StartReason = startReason;
            TotalEnemies = totalEnemies;
            PlayerStartHealth = playerStartHealth;
            PlayerStartAmmo = playerStartAmmo;
            Location = location;
        }
    }

    public class EncounterStartedTelemetry : EncounterTelemetry
    {
        public EncounterStartedTelemetry(string sessionId, EncounterStartedTelemetryData data) : base(sessionId,
            TelemetryEventType.EncounterStarted, data)
        {
            Data.Add("StartReason", data.StartReason);
            Data.Add("TotalEnemies", data.TotalEnemies);
            Data.Add("PlayerStartHealth", data.PlayerStartHealth);
            Data.Add("PlayerStartAmmo", data.PlayerStartAmmo);
            Data.Add("Location", data.Location?.Location);
        }
    }
}