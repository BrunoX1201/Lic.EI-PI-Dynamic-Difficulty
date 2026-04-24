using UnityEngine;

namespace Unity.FPS.Game
{
    [CreateAssetMenu(fileName = "TelemetryServiceSettingsSO",
        menuName = "Scriptable Objects/TelemetryServiceSettingsSO")]
    public class TelemetryServiceSettingsSO : ScriptableObject
    {
        public int BatchSize = 10;
        public string UploaderBaseFilePath { get; private set; }

        private void OnEnable()
        {
            UploaderBaseFilePath = $@"{Application.persistentDataPath}/Telemetry/Events";
        }
    }
}