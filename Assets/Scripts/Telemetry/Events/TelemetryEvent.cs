using System;
using System.Collections.Generic;

namespace Telemetry.Events
{
    public abstract class TelemetryEvent : ITelemetryEvent
    {
        public TelemetryEventType EventType { get; }
        public Dictionary<string, object> Data { get; } = new();
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        protected int m_sessionId;

        protected TelemetryEvent(int sessionId, TelemetryEventType eventType)
        {
            m_sessionId = sessionId;
            EventType = eventType;

            Data.Add("SessionId", sessionId);
            Data.Add("EventType", eventType);
            Data.Add("Timestamp", Timestamp);
        }
    }
}