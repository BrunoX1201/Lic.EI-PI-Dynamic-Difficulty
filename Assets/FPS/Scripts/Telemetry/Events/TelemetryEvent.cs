using System;
using System.Collections.Generic;

namespace Unity.FPS.Telemetry
{
    public abstract class TelemetryEvent : ITelemetryEvent
    {
        public TelemetryEventType EventType { get; }
        public Dictionary<string, object> Data { get; } = new();
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        protected string m_sessionId;

        protected TelemetryEvent(string sessionId, TelemetryEventType eventType)
        {
            m_sessionId = sessionId;
            EventType = eventType;

            Data.Add("SessionId", sessionId);
            Data.Add("EventType", eventType);
            Data.Add("Timestamp", Timestamp);
        }
    }
}