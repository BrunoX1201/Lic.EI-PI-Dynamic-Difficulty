using UnityEngine;

namespace Unity.FPS.DDA
{
    public class DDAService
    {
        private static DDAService s_instance = new();
        public static DDAService Instance => s_instance;
        private bool m_isInitialized;
        
        public void Initialize()
        {
            if (m_isInitialized)
            {
                return;
            }
            
            DDAModifierState.Initialize();
            m_isInitialized = true;
        }

        public void NotifyEncounterCompleted(string encounterId)
        {
            Debug.Log($"[DDAService] EncounterCompleted: {encounterId}");
            DDAEventManager.Broadcast(new EncounterCompleted(encounterId));
        }
    }
}