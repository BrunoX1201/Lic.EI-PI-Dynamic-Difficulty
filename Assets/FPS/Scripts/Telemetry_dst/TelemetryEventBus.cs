using System;
using System.Collections.Generic;
using Telemetry.Events;
using UnityEngine;

namespace Telemetry
{
    public static class TelemetryEventBus
    {
        public static bool IsDebugOn = false;
        private static readonly object _lock = new();
        private static readonly List<Action<ITelemetryEvent>> _listeners = new();

        public static void Subscribe(Action<ITelemetryEvent> handler)
        {
            lock (_lock)
            {
                if (_listeners.Contains(handler)) return;
                _listeners.Add(handler);
            }
        }

        public static void Unsubscribe(Action<ITelemetryEvent> handler)
        {
            lock (_lock)
            {
                if (!_listeners.Contains(handler)) return;

                _listeners.Remove(handler);
            }
        }

        public static void Publish(ITelemetryEvent evt)
        {
            if (IsDebugOn)
            {
                string msg = "Event: " + evt + "\n \n Data: ";
                foreach (KeyValuePair<string, object> item in evt.Data)
                {
                    msg += $"\n  {item.Key}: {item.Value}";
                }

                Debug.Log(msg);
            }

            List<Action<ITelemetryEvent>> listenersCopy;

            lock (_lock)
            {
                listenersCopy = new List<Action<ITelemetryEvent>>(_listeners);
            }

            foreach (Action<ITelemetryEvent> listener in listenersCopy)
            {
                listener?.Invoke(evt);
            }
        }
    }
}