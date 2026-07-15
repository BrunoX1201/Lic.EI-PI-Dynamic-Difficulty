using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.FPS.API
{
    public class ApiClient : IApiClient
    {
        public async Task<ApiResult<IProcessEncounterResponse>> ProcessEncounter(IProcessEncounterRequest request)
        {
            string jsonPayload = JsonConvert.SerializeObject(request);

            using UnityWebRequest webRequest = new(ApiConfig.K_SERVER_URL, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[DDAApiClient] POST {ApiConfig.K_SERVER_URL}\n{jsonPayload}");
            await webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                return ApiResult<IProcessEncounterResponse>.Fail(webRequest.error);

            return ParseResponse(webRequest.downloadHandler.text);
        }

        private static ApiResult<IProcessEncounterResponse> ParseResponse(string json)
        {
            try
            {
                ProcessEncounterResponse response = JsonConvert.DeserializeObject<ProcessEncounterResponse>(json);

                if (response == null || response.Status != "success" || response.ActionParams == null)
                    return ApiResult<IProcessEncounterResponse>.Fail("Unexpected response format.");

                return ApiResult<IProcessEncounterResponse>.Ok(response);
            }
            catch (Exception e)
            {
                return ApiResult<IProcessEncounterResponse>.Fail(e.Message);
            }
        }
    }
}