using System;
using System.Collections.Generic;

namespace Telemetry
{
    public abstract class TelemetryEvent : ITelemetryEvent
    {
        public TelemetryEventType EventType { get; }
        public Dictionary<string, object> Data { get; } = new();
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        protected int _sessionId;

        protected TelemetryEvent(int sessionId, TelemetryEventType eventType)
        {
            _sessionId = sessionId;
            EventType = eventType;

            Data.Add("SessionId", sessionId);
            Data.Add("EventType", eventType);
            Data.Add("Timestamp", Timestamp);
        }
    }
}