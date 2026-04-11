using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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

            LogEvent(evt);
            TelemetryEventBus.Publish(evt);
        }

        public static void TrackPlayerAttack(int sessionId, PlayerAttackedTelemetryData eventData)
        {
            PlayerAttackedTelemetry evt = new(sessionId, eventData);

            LogEvent(evt);
            TelemetryEventBus.Publish(evt);
        }

        private static void LogEvent(ITelemetryEvent evt)
        {
            string msg = $"[TELEMETRY] Event: {evt.GetType().Name}\n\nData:";
            foreach (KeyValuePair<string, object> item in evt.Data)
            {
                msg += $"\n\t{item.Key} => {item.Value}";
            }

            Debug.Log(msg);
        }
    }
}