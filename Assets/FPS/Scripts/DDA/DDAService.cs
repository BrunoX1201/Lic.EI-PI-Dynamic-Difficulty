using UnityEngine;

namespace Unity.FPS.DDA
{
    public class DDAService
    {
        private static DDAService s_instance = new();
        public static DDAService Instance => s_instance;

        static DDAService()
        {
            DDAModifierState.Initialize();
        }

        public void NotifyEncounterCompleted(string encounterId)
        {
            Debug.Log($"[DDAService] EncounterCompleted: {encounterId}");
            DDAEventManager.Broadcast(new EncounterCompleted(encounterId));
        }
    }
}