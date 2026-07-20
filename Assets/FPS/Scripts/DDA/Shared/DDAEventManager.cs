using System;
using System.Collections.Generic;

namespace Unity.FPS.DDA
{
    public class DDAEvent
    {
    }

    public static class DDAEventManager
    {
        private static readonly Dictionary<Type, Action<DDAEvent>> s_Events = new();

        private static readonly Dictionary<Delegate, Action<DDAEvent>> s_EventLookups = new();

        public static void AddListener<T>(Action<T> evt) where T : DDAEvent
        {
            if (!s_EventLookups.ContainsKey(evt))
            {
                Action<DDAEvent> newAction = (e) => evt((T)e);
                s_EventLookups[evt] = newAction;

                if (s_Events.TryGetValue(typeof(T), out Action<DDAEvent> internalAction))
                    s_Events[typeof(T)] = internalAction += newAction;
                else
                    s_Events[typeof(T)] = newAction;
            }
        }

        public static void RemoveListener<T>(Action<T> evt) where T : DDAEvent
        {
            if (s_EventLookups.TryGetValue(evt, out Action<DDAEvent> action))
            {
                if (s_Events.TryGetValue(typeof(T), out Action<DDAEvent> tempAction))
                {
                    tempAction -= action;
                    if (tempAction == null)
                        s_Events.Remove(typeof(T));
                    else
                        s_Events[typeof(T)] = tempAction;
                }

                s_EventLookups.Remove(evt);
            }
        }

        public static void Broadcast(DDAEvent evt)
        {
            if (s_Events.TryGetValue(evt.GetType(), out Action<DDAEvent> action))
                action.Invoke(evt);
        }

        public static void Clear()
        {
            s_Events.Clear();
            s_EventLookups.Clear();
        }
    }
}