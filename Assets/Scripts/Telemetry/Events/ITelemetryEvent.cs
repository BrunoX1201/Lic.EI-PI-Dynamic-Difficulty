using System;
using System.Collections.Generic;

namespace Telemetry.Events
{
    public interface ITelemetryEvent
    {
        TelemetryEventType EventType { get; }
        Dictionary<string, object> Data { get; }
        DateTime Timestamp { get; }
    }
}