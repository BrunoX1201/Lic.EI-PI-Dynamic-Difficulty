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
            public int TotalEnemies;
            public float EnemyHealth;
            public float EnemyHitbox;
        }

        [Serializable]
        private class ApiResponse
        {
            public string Status;
            public string Agent;
            public string Action;
            public ActionParams ActionParams;
        }

        private static string s_encounterId;

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

            s_encounterId = evt.EncounterId;
            ProcessResponse(request.downloadHandler.text);
        }

        private static void ProcessResponse(string json)
        {
            try
            {
                ApiResponse response = JsonUtility.FromJson<ApiResponse>(json);

                if (response.Status != "success" || response.ActionParams == null)
                    throw new Exception("Default modifiers will be kept.");

                Debug.Log(
                    $"[DDA] Model output received successfully (Agent={response.Agent}, Action={response.Action}).");

                DDAEventManager.Broadcast(new DDAModelOutputReceived(
                    response.ActionParams.TotalEnemies,
                    response.ActionParams.EnemyHealth,
                    response.ActionParams.EnemyHitbox,
                    response.Agent,
                    response.Action
                ));
            }
            catch (Exception e)
            {
                Debug.LogError($"[DDA] Failed to process response: {e.Message}");
                DDAEventManager.Broadcast(new DDAPredictionFailed(s_encounterId));
            }
        }
    }
}