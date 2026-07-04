using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.FPS.DDA
{
    public static class DDAController
    {
        [Serializable]
        private class ActionParams
        {
            public int total_enemies;
            public float enemy_health;
            public float enemy_hitbox;
        }

        [Serializable]
        private class ApiResponse
        {
            public string status;
            public string agent;
            public string action;
            public ActionParams action_params;
        }

        private static string m_encounterId;

        public static void Initialize()
        {
            DDAEventManager.AddListener<EncounterCompleted>(OnEncounterCompleted);
        }

        private static void OnEncounterCompleted(EncounterCompleted evt)
        {
            _ = RequestPrediction(evt);
        }

        private static async Task RequestPrediction(EncounterCompleted evt)
        {
            string jsonPayload = $"{{\"encounter_id\":\"{evt.EncounterId}\"}}";

            using UnityWebRequest request = new(DDAConfig.SERVER_URL, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[DDA] Requesting adjustment from encounter: {evt.EncounterId}");
            await request.SendWebRequest();

            if (request.result is UnityWebRequest.Result.ConnectionError
                or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[DDA] API error: {request.error}. Default modifiers will be kept.");
                DDAEventManager.Broadcast(new DDAPredictionFailed(evt.EncounterId));
                return;
            }

            m_encounterId = evt.EncounterId;
            ProcessResponse(request.downloadHandler.text);
        }

        private static void ProcessResponse(string json)
        {
            try
            {
                ApiResponse response = JsonUtility.FromJson<ApiResponse>(json);

                if (response.status != "success" || response.action_params == null)
                    throw new Exception("Default modifiers will be kept.");

                Debug.Log(
                    $"[DDA] Model output received successfully (agent={response.agent}, action={response.action}).");

                DDAEventManager.Broadcast(new DDAModelOutputReceived(
                    response.action_params.total_enemies,
                    response.action_params.enemy_health,
                    response.action_params.enemy_hitbox,
                    response.agent,
                    response.action
                ));
            }
            catch (Exception e)
            {
                Debug.LogError($"[DDA] Failed to process response: {e.Message}");
                DDAEventManager.Broadcast(new DDAPredictionFailed(m_encounterId));
            }
        }
    }
}