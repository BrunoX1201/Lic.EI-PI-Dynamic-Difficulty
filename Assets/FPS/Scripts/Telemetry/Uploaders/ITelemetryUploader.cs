using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.FPS.Telemetry
{
    public interface ITelemetryUploader
    {
        Task UploadAsync(IEnumerable<ITelemetryEvent> events);
    }
}