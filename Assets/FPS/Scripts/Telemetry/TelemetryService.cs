using System.Threading.Tasks;

namespace Unity.FPS.Telemetry
{
    public static class TelemetryService
    {
        private static TelemetryBatcher s_batcher;

        public static void Initialize(ITelemetryUploader uploader = null, int batchSize = 10)
        {
            s_batcher = new TelemetryBatcher(batchSize, uploader);
        }

        public static async Task FlushAsync()
        {
            if (s_batcher == null)
            {
                return;
            }

            await s_batcher.FlushAsync();
        }

        public static async ValueTask ShutdownAsync()
        {
            await s_batcher.DisposeAsync();
            s_batcher = null;
        }

        public static void Shutdown()
        {
            s_batcher.Dispose();
            s_batcher = null;
        }

        public static void TrackPlayerDeath(string sessionId, PlayerDiedTelemetryData eventData)
        {
            PlayerDiedTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackPlayerAttack(string sessionId, PlayerAttackedTelemetryData eventData)
        {
            PlayerAttackedTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackItemPickUp(string sessionId, ItemPickedUpTelemetryData eventData)
        {
            ItemPickedUpTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackNewLocationDiscover(string sessionId, NewLocationDiscoveredTelemetryData eventData)
        {
            NewLocationDiscoveredTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackTargetKill(string sessionId, TargetKilledTelemetryData eventData)
        {
            TargetKilledTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackPlayerTakeDamage(string sessionId, PlayerTookDamageTelemetryData eventData)
        {
            PlayerTookDamageTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackGameTimePass(string sessionId, GameTimePassedTelemetryData eventData)
        {
            GameTimePassedTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackPlayerGoAirborne(string sessionId, PlayerWentAirborneTelemetryData eventData)
        {
            PlayerWentAirborneTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackEncounterStart(string sessionId, EncounterStartedTelemetryData eventData)
        {
            EncounterStartedTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackEncounterEnd(string sessionId, EncounterEndedTelemetryData eventData)
        {
            EncounterEndedTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackEncounterZoneLeave(string sessionId, EncounterZoneLeftTelemetryData eventData)
        {
            EncounterZoneLeftTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackEncounterZoneEnter(string sessionId, EncounterZoneEnteredTelemetryData eventData)
        {
            EncounterZoneEnteredTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }
    }
}