using System;
using Newtonsoft.Json;

namespace Unity.FPS.API
{
    [Serializable]
    public class ProcessEncounterRequest : IRequest
    {
        [JsonProperty("encounter_id")] public string EncounterId { get; set; }

        [JsonProperty("next_encounter_restrictions")]
        public EncounterRestrictions NextEncounterRestrictions { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public RequestOptions Options { get; set; }
    }

    [Serializable]
    public class EncounterRestrictions
    {
        [JsonProperty("turret")] public EnemyTypeRestrictions Turret { get; set; }

        [JsonProperty("mobile")] public EnemyTypeRestrictions Mobile { get; set; }
    }

    [Serializable]
    public class EnemyTypeRestrictions
    {
        [JsonProperty("count")] public LimitValue<int> Count { get; set; }

        [JsonProperty("health")] public LimitValue<float> Health { get; set; }

        [JsonProperty("hitbox")] public LimitValue<float> Hitbox { get; set; }
    }


    [Serializable]
    public class LimitValue<T>
    {
        [JsonProperty("min_limit")] public T MinLimit;

        [JsonProperty("max_limit")] public T MaxLimit;

        [JsonProperty("default_value")] public T DefaultValue;

        [JsonProperty("previous_value")] public T PreviousValue;

        public LimitValue()
        {
        }

        public LimitValue(T minLimit, T maxLimit, T defaultValue, T previousValue)
        {
            MinLimit = minLimit;
            MaxLimit = maxLimit;
            DefaultValue = defaultValue;
            PreviousValue = previousValue;
        }
    }

    [Serializable]
    public class RequestOptions
    {
        [JsonProperty("rollback_on_success")] public bool RollbackOnSuccess { get; set; }
    }
}