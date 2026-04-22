using System.IO;
using Unity.FPS.Telemetry;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class TelemetryManager : Singleton<TelemetryManager>
    {
        [SerializeField] private TelemetryServiceSettingsSO m_telemetryServiceSettings;

        public TelemetryServiceSettingsSO TelemetryServiceSettings => m_telemetryServiceSettings;

        public override void Awake()
        {
            base.Awake();

            if (!Directory.Exists(m_telemetryServiceSettings.UploaderBaseFilePath))
            {
                Directory.CreateDirectory(m_telemetryServiceSettings.UploaderBaseFilePath);
            }

            TelemetryService.Initialize(
                new CSVFileTelemetryUploader(
                    new FileTelemetryUploaderSettings(m_telemetryServiceSettings.UploaderBaseFilePath)),
                m_telemetryServiceSettings.BatchSize);
        }

        private void OnApplicationQuit()
        {
            TelemetryService.Shutdown();
        }
    }
}