using Newtonsoft.Json;

namespace Unity.FPS.API
{
    public interface IProcessEncounterResponse
    {
        string Status { get; }
        string Agent { get; }
        string Action { get; }
        ActionParams ActionParams { get; }
    }

    [System.Serializable]
    public class ProcessEncounterResponse : IProcessEncounterResponse
    {
        [JsonProperty("status")] public string Status { get; set; }

        [JsonProperty("agent")] public string Agent { get; set; }

        [JsonProperty("action")] public string Action { get; set; }

        [JsonProperty("action_params")] public ActionParams ActionParams { get; set; }
    }

    [System.Serializable]
    public class ActionParams
    {
        [JsonProperty("turret")] public EnemyTypeParams Turret { get; set; }

        [JsonProperty("mobile")] public EnemyTypeParams Mobile { get; set; }
    }

    [System.Serializable]
    public class EnemyTypeParams
    {
        [JsonProperty("health")] public float Health { get; set; }

        [JsonProperty("hitbox")] public float Hitbox { get; set; }

        [JsonProperty("count")] public int Count { get; set; }
    }
}