using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.FPS.API
{
    public class APIService
    {
        public static APIService Instance { get; } = new();

        private bool m_isInitialized;

        public async Task Initialize(string sessionId)
        {
            if (m_isInitialized)
            {
                Debug.Log("[API] Service already initialized");
                return;
            }

            Debug.Log("[API] Service initializing...");
            SetupSessionRequest request = new()
            {
                SessionId = sessionId
            };

            APIResult<SetupSessionResponse> res = await SetupSession(request);

            if (!res.Success)
            {
                Debug.LogError("[API] Could not initialize API service");
            }

            Debug.Log("[API] Service initialized");
            m_isInitialized = true;
        }

        public static async Task<APIResult<ProcessEncounterResponse>> ProcessEncounter(ProcessEncounterRequest request)
        {
            try
            {
                string jsonPayload = JsonConvert.SerializeObject(request);
                using UnityWebRequest webRequest = new(APIConfig.PROCESS_ENCOUNTER_ENDPOINT, "POST");
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                string res = await SendRequest(webRequest);

                ProcessEncounterResponse convert_res = ConvertResponse<ProcessEncounterResponse>(res);
                if (convert_res.ActionParams == null)
                {
                    throw new Exception("Invalid response.");
                }

                return APIResult<ProcessEncounterResponse>.Ok(convert_res);
            }
            catch (Exception e)
            {
                Debug.LogError($"[API] Error: {e.Message}");
                return APIResult<ProcessEncounterResponse>.Fail(e.Message);
            }
        }

        private static async Task<APIResult<SetupSessionResponse>> SetupSession(SetupSessionRequest request)
        {
            try
            {
                string jsonPayload = JsonConvert.SerializeObject(request);
                using UnityWebRequest webRequest = new(APIConfig.SESSION_ENDPOINT, "POST");
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                string res = await SendRequest(webRequest);

                SetupSessionResponse convert_res = ConvertResponse<SetupSessionResponse>(res);

                return APIResult<SetupSessionResponse>.Ok(convert_res);
            }
            catch (Exception e)
            {
                Debug.LogError($"[API] Error: {e.Message}");
                return APIResult<SetupSessionResponse>.Fail(e.Message);
            }
        }

        private static async Task<string> SendRequest(UnityWebRequest request)
        {
            Debug.Log(
                $"[API] Sending {request.url} ({request.method}):\n{Encoding.UTF8.GetString(request.uploadHandler.data)}");

            await request.SendWebRequest();
            if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                throw new Exception(request.error);
            }

            return request.downloadHandler.text;
        }

        private static T ConvertResponse<T>(string jsonResponse) where T : IResponse
        {
            T convertedResponse = JsonConvert.DeserializeObject<T>(jsonResponse);

            if (convertedResponse == null || convertedResponse.Status != "success")
            {
                throw new Exception("Invalid response.");
            }

            return convertedResponse;
        }
    }
}