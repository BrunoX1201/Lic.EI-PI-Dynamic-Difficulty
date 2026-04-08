using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "TelemetryServiceSettingsSO",
        menuName = "Scriptable Objects/TelemetryServiceSettingsSO")]
    public class TelemetryServiceSettingsSO : ScriptableObject
    {
        public int BatchSize = 10;
    }
}