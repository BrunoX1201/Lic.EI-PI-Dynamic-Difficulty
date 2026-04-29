namespace Unity.FPS.Telemetry
{
    public enum TelemetryEventType
    {
        Unknown = 0,
        PlayerDied = 1,
        PlayerAttacked = 2,
        NewLocationDiscovered = 3,
        PlayerWentAirborne = 4,
        GameTimePassed = 5,
        PlayerTookDamage = 6,
        TargetKilled = 7,
        ItemPickedUp = 8,
        EncounterStarted = 9,
        EncounterEnded = 10,
        EncounterZoneLeft = 11,
        EncounterZoneEntered = 12
    }
}