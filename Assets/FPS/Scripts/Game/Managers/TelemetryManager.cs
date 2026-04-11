using Telemetry;
using Telemetry.Uploaders;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class TelemetryManager : Singleton<TelemetryManager>
    {
        [SerializeField] private int m_BatchSize = 10;

        public override void Awake()
        {
            base.Awake();
            TelemetryService.Initialize(new ConsoleTelemetryUploader(), m_BatchSize);
        }
    }
}