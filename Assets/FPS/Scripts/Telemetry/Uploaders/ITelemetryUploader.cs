using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.FPS.Telemetry
{
    public interface ITelemetryUploader : IDisposable, IAsyncDisposable
    {
        Task UploadAsync(IEnumerable<ITelemetryEvent> events);
    }
}