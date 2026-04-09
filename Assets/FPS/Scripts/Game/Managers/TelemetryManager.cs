using Unity.FPS.Telemetry;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class TelemetryManager : Singleton<TelemetryManager>
    {
        [SerializeField] private TelemetryServiceSettingsSO m_telemetryServiceSettings;

        public override void Awake()
        {
            base.Awake();
            TelemetryService.Initialize(new ConsoleTelemetryUploader(), m_telemetryServiceSettings.BatchSize);
        }
    }
}