using System;
using System.Collections.Generic;

namespace Telemetry
{
    public interface ITelemetryEvent
    {
        TelemetryEventType EventType { get; }
        Dictionary<string, object> Data { get; }
        DateTime Timestamp { get; }
    }
}