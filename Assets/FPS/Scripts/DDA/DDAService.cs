using UnityEngine;

namespace Unity.FPS.DDA
{
    public class DDAService
    {
        public static DDAService Instance { get; } = new();

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