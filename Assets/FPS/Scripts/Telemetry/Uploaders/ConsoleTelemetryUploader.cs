using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Unity.FPS.Telemetry
{
    public class ConsoleTelemetryUploader : ITelemetryUploader
    {
        public Task UploadAsync(IEnumerable<ITelemetryEvent> events)
        {
            foreach (ITelemetryEvent evt in events)
            {
                string json = JsonConvert.SerializeObject(evt.Data);
                Console.WriteLine($"[TELEMETRY] {evt.EventType} at {evt.Timestamp:HH:mm:ss} - {json}");
            }

            return Task.CompletedTask;
        }
    }
}