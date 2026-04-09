using System.Threading.Tasks;
using Telemetry.Events;
using Telemetry.Uploaders;

namespace Telemetry
{
    public static class TelemetryService
    {
        private static TelemetryBatcher _batcher;

        public static void Initialize(ITelemetryUploader uploader = null, int batchSize = 10)
        {
            _batcher = new TelemetryBatcher(batchSize, uploader);
        }

        public static async ValueTask ShutdownAsync()
        {
            await _batcher.DisposeAsync();
            _batcher = null;
        }

        public static void Shutdown()
        {
            _batcher.Dispose();
            _batcher = null;
        }

        public static void TrackPlayerAttack(int sessionId, PlayerAttackedTelemetryData telemetryData)
        {
            PlayerAttackedTelemetry telemetryEvent = new(
                sessionId,
                telemetryData);

            TelemetryEventBus.Publish(telemetryEvent);
        }
    }
}