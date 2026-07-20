using System;
using Newtonsoft.Json;

namespace Unity.FPS.API
{
    [Serializable]
    public class SetupSessionRequest : IRequest
    {
        [JsonProperty("session_id")] public string SessionId { get; set; }
        [JsonProperty("telemetry_base_path")] public string TelemetryBasePath { get; set; }
    }
}