using System.Threading.Tasks;
using Telemetry.Uploaders;

namespace Telemetry
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
    }
}