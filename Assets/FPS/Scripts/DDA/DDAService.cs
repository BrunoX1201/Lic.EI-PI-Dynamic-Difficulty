using UnityEngine;

namespace Unity.FPS.DDA
{
    public class DDAService
    {
        private static DDAService m_instance = new();
        public static DDAService Instance => m_instance;

        static DDAService()
        {
            DDAModifierState.Initialize();
            DDAController.Initialize();
        }

        public void NotifyEncounterCompleted(string encounterId)
        {
            Debug.Log($"[DDAService] EncounterCompleted: {encounterId}");
            DDAEventManager.Broadcast(new EncounterCompleted(encounterId));
        }
    }
}