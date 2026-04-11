using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Telemetry
{
    public static class TelemetryEventBus
    {
        public static bool IsDebugOn = false;
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
            if (IsDebugOn)
            {
                LogEvent(evt);
            }

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

        private static void LogEvent(ITelemetryEvent evt)
        {
            string msg = $"[TELEMETRY] Event: {evt.GetType().Name}\n\nData:";
            foreach (KeyValuePair<string, object> item in evt.Data)
            {
                msg += $"\n\t{item.Key} => {item.Value}";
            }

            Debug.Log(msg);
        }
    }
}