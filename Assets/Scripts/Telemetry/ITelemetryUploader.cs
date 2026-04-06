using System.Collections.Generic;
using System.Threading.Tasks;

namespace Telemetry
{
    public interface ITelemetryUploader
    {
        Task UploadAsync(IEnumerable<ITelemetryEvent> events);
    }
}