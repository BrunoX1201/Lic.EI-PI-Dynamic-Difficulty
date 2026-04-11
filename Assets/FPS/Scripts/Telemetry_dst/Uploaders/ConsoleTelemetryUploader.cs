using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telemetry.Events;

namespace Telemetry.Uploaders
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