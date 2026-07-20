using System;
using Newtonsoft.Json;

namespace Unity.FPS.API
{
    [Serializable]
    public class ProcessEncounterResponse : Response
    {
        [JsonProperty("agent")] public string Agent { get; set; }

        [JsonProperty("action")] public string Action { get; set; }

        [JsonProperty("action_params")] public ActionParams ActionParams { get; set; }
    }

    [Serializable]
    public class ActionParams
    {
        [JsonProperty("turret")] public EnemyTypeParams Turret { get; set; }

        [JsonProperty("mobile")] public EnemyTypeParams Mobile { get; set; }
    }

    [Serializable]
    public class EnemyTypeParams
    {
        [JsonProperty("health")] public float Health { get; set; }

        [JsonProperty("hitbox")] public float Hitbox { get; set; }

        [JsonProperty("count")] public int Count { get; set; }
    }
}