using Core;
using ScriptableObjects;
using Telemetry;
using Telemetry.Uploaders;
using UnityEngine;

public class TelemetryManager : Singleton<TelemetryManager>
{
    [SerializeField] private TelemetryServiceSettingsSO m_telemetryServiceSettings;

    public override void Awake()
    {
        base.Awake();
        TelemetryService.Initialize(new ConsoleTelemetryUploader(), m_telemetryServiceSettings.BatchSize);
    }
}