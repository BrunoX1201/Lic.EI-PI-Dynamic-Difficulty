using System;
using System.Collections.Generic;

namespace Unity.FPS.Telemetry
{
    public static class TelemetryEventBus
    {
        private static readonly object s_lock = new();
        private static readonly List<Action<ITelemetryEvent>> s_listeners = new();

        public static void Subscribe(Action<ITelemetryEvent> handler)
        {
            lock (s_lock)
            {
                if (s_listeners.Contains(handler))
                {
                    return;
                }

                s_listeners.Add(handler);
            }
        }

        public static void Unsubscribe(Action<ITelemetryEvent> handler)
        {
            lock (s_lock)
            {
                if (!s_listeners.Contains(handler))
                {
                    return;
                }

                s_listeners.Remove(handler);
            }
        }

        public static void Publish(ITelemetryEvent evt)
        {
            List<Action<ITelemetryEvent>> listenersCopy;

            lock (s_lock)
            {
                listenersCopy = new List<Action<ITelemetryEvent>>(s_listeners);
            }

            foreach (Action<ITelemetryEvent> listener in listenersCopy)
            {
                listener?.Invoke(evt);
            }
        }
    }
}