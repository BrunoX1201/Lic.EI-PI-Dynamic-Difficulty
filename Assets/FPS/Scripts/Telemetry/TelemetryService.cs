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

        public static void TrackPlayerDeath(int sessionId, PlayerDiedTelemetryData eventData)
        {
            PlayerDiedTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackPlayerAttack(int sessionId, PlayerAttackedTelemetryData eventData)
        {
            PlayerAttackedTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackItemPickUp(int sessionId, ItemPickedUpTelemetryData eventData)
        {
            ItemPickedUpTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackNewLocationDiscover(int sessionId, NewLocationDiscoveredTelemetryData eventData)
        {
            NewLocationDiscoveredTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }

        public static void TrackPlayerTakeDamage(int sessionId, PlayerTookDamageTelemetryData eventData)
        {
            PlayerTookDamageTelemetry evt = new(sessionId, eventData);

            TelemetryEventBus.Publish(evt);
        }
    }
}