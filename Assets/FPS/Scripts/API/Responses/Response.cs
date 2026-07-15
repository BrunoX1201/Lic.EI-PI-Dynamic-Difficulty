using System;
using Newtonsoft.Json;

namespace Unity.FPS.API
{
    [Serializable]
    public class Response : IResponse
    {
        [JsonProperty("status")] public string Status { get; set; }
    }
}