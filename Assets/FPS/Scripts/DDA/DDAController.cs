using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.FPS.DDA
{
    public static class DDAController
    {
        [System.Serializable]
        private class EnemyTypeParams
        {
            public float Health;
            public float Hitbox;
            public int Count;
        }

        [System.Serializable]
        private class ActionParams
        {
            public EnemyTypeParams Turret;
            public EnemyTypeParams Mobile;
        }

        [System.Serializable]
        private class ApiResponse
        {
            public string Status;
            public string Agent;
            public string Action;
            public ActionParams ActionParams;
        }

        public static void RequestPrediction(string encounterId, DDAEncounterRestrictions next,
            bool? rollbackOnSuccess = null)
        {
            _ = SendRequest(encounterId, next, rollbackOnSuccess);
        }

        private static async Task SendRequest(string encounterId, DDAEncounterRestrictions next,
            bool? rollbackOnSuccess)
        {
            string jsonPayload = BuildPayload(encounterId, next, rollbackOnSuccess);

            using UnityWebRequest request = new(DDAConfig.K_SERVER_URL, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[DDA] Requesting adjustment from encounter: {encounterId}\n{jsonPayload}");
            await request.SendWebRequest();

            if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[DDA] API error: {request.error}. Default modifiers will be kept.");
                DDAEventManager.Broadcast(new DDAPredictionFailed(encounterId));
                return;
            }

            ProcessResponse(encounterId, request.downloadHandler.text);
        }

        private static string BuildPayload(string encounterId, DDAEncounterRestrictions next, bool? rollbackOnSuccess)
        {
            float previousMobileHp = DDAModifierState.MobileHealthModifier.Value;
            float previousMobileHitbox = DDAModifierState.MobileHitboxModifier.Value;
            float previousTurretHp = DDAModifierState.TurretHealthModifier.Value;
            float previousTurretHitbox = DDAModifierState.TurretHitboxModifier.Value;

            string F(float v)
            {
                return v.ToString(CultureInfo.InvariantCulture);
            }

            string json = "{" +
                          $"\"encounter_id\":\"{encounterId}\"," +
                          "\"next_encounter_restrictions\":{" +
                          $"\"default\":{next.DefaultMobiles}," +
                          $"\"min_enemies\":{next.MinMobiles}," +
                          $"\"max_enemies\":{next.MaxMobiles}," +
                          $"\"default_bosses\":{next.DefaultBosses}," +
                          $"\"min_bosses\":{next.MinBosses}," +
                          $"\"max_bosses\":{next.MaxBosses}," +
                          $"\"previous_mobile_hp\":{F(previousMobileHp)}," +
                          $"\"min_mobile_hp\":{F(next.MinMobileHealth)}," +
                          $"\"max_mobile_hp\":{F(next.MaxMobileHealth)}," +
                          $"\"previous_mobile_hitbox\":{F(previousMobileHitbox)}," +
                          $"\"min_mobile_hitbox\":{F(next.MinMobileHitbox)}," +
                          $"\"max_mobile_hitbox\":{F(next.MaxMobileHitbox)}," +
                          $"\"previous_turret_hp\":{F(previousTurretHp)}," +
                          $"\"min_turret_hp\":{F(next.MinTurretHealth)}," +
                          $"\"max_turret_hp\":{F(next.MaxTurretHealth)}," +
                          $"\"previous_turret_hitbox\":{F(previousTurretHitbox)}," +
                          $"\"min_turret_hitbox\":{F(next.MinTurretHitbox)}," +
                          $"\"max_turret_hitbox\":{F(next.MaxTurretHitbox)}" +
                          "}";

            if (rollbackOnSuccess.HasValue)
                json += $",\"options\":{{\"rollback_on_success\":{(rollbackOnSuccess.Value ? "true" : "false")}}}";

            json += "}";
            return json;
        }

        private static void ProcessResponse(string encounterId, string json)
        {
            try
            {
                ApiResponse response = JsonUtility.FromJson<ApiResponse>(json);

                if (response.Status != "success" || response.ActionParams == null)
                    throw new System.Exception("Unexpected response format.");

                Debug.Log(
                    $"[DDA] Model output received successfully (Agent={response.Agent}, Action={response.Action}).");

                DDAEventManager.Broadcast(new DDAModelOutputReceived(
                    response.ActionParams.Mobile.Count,
                    response.ActionParams.Turret.Count,
                    response.ActionParams.Mobile.Health,
                    response.ActionParams.Mobile.Hitbox,
                    response.ActionParams.Turret.Health,
                    response.ActionParams.Turret.Hitbox,
                    response.Agent,
                    response.Action
                ));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DDA] Failed to process response: {e.Message}. Default modifiers will be kept.");
                DDAEventManager.Broadcast(new DDAPredictionFailed(encounterId));
            }
        }
    }
}