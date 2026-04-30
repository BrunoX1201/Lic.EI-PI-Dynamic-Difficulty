namespace Unity.FPS.Telemetry
{
    public enum EncounterEndReason
    {
        Completed = 0,
        PlayerDied = 1
    }

    public class EncounterEndedTelemetryData : EncounterTelemetryData
    {
        public EncounterEndReason EndReason { get; }
        public float PlayerRemainingHealth { get; }
        public float PlayerRemainingAmmo { get; }
        public int RemainingEnemies { get; }

        public EncounterEndedTelemetryData(string id, EncounterEndReason endReason, float playerRemainingHealth,
            float playerRemainingAmmo, int remainingEnemies) : base(id)
        {
            EndReason = endReason;
            PlayerRemainingHealth = playerRemainingHealth;
            PlayerRemainingAmmo = playerRemainingAmmo;
            RemainingEnemies = remainingEnemies;
        }
    }

    public class EncounterEndedTelemetry : EncounterTelemetry
    {
        public EncounterEndedTelemetry(string sessionId, EncounterEndedTelemetryData data) : base(sessionId,
            TelemetryEventType.EncounterEnded, data)
        {
            Data.Add("EndReason", data.EndReason);
            Data.Add("PlayerRemainingHealth", data.PlayerRemainingHealth);
            Data.Add("PlayerRemainingAmmo", data.PlayerRemainingAmmo);
            Data.Add("RemainingEnemies", data.RemainingEnemies);
        }
    }
}