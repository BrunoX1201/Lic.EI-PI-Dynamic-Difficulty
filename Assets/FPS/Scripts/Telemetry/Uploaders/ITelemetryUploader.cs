using System.Collections.Generic;
using System.Threading.Tasks;
using Telemetry.Events;

namespace Telemetry.Uploaders
{
    public interface ITelemetryUploader
    {
        Task UploadAsync(IEnumerable<ITelemetryEvent> events);
    }
}