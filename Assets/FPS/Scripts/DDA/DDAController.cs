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
            public float health;
            public float hitbox;
            public int count;
        }

        [System.Serializable]
        private class ActionParams
        {
            public EnemyTypeParams turret;
            public EnemyTypeParams mobile;
        }

        [System.Serializable]
        private class ApiResponse
        {
            public string status;
            public string agent;
            public string action;
            public ActionParams action_params;
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

            // Funções auxiliares para garantir o ponto decimal (.) em vez da vírgula (,)
            string F(float v) => v.ToString(CultureInfo.InvariantCulture);
            string I(int v) => v.ToString(CultureInfo.InvariantCulture);

            StringBuilder sb = new StringBuilder();

            sb.Append("{");
            sb.Append($"\"encounter_id\":\"{encounterId}\",");
            sb.Append("\"next_encounter_restrictions\":{");
                
                // Objeto TURRET
                sb.Append("\"turret\":{");
                sb.Append($"\"count\":{{\"min_limit\":{I(next.MinBosses)},\"max_limit\":{I(next.MaxBosses)},\"previous_value\":{I(next.DefaultBosses)}}},");
                sb.Append($"\"health\":{{\"min_limit\":{F(next.MinTurretHealth)},\"max_limit\":{F(next.MaxTurretHealth)},\"previous_value\":{F(previousTurretHp)}}},");
                sb.Append($"\"hitbox\":{{\"min_limit\":{F(next.MinTurretHitbox)},\"max_limit\":{F(next.MaxTurretHitbox)},\"previous_value\":{F(previousTurretHitbox)}}}");
                sb.Append("},"); // Fecha turret

                // Objeto MOBILE
                sb.Append("\"mobile\":{");
                sb.Append($"\"count\":{{\"min_limit\":{I(next.MinMobiles)},\"max_limit\":{I(next.MaxMobiles)},\"previous_value\":{I(next.DefaultMobiles)}}},");
                sb.Append($"\"health\":{{\"min_limit\":{F(next.MinMobileHealth)},\"max_limit\":{F(next.MaxMobileHealth)},\"previous_value\":{F(previousMobileHp)}}},");
                sb.Append($"\"hitbox\":{{\"min_limit\":{F(next.MinMobileHitbox)},\"max_limit\":{F(next.MaxMobileHitbox)},\"previous_value\":{F(previousMobileHitbox)}}}");
                sb.Append("}"); // Fecha mobile

            sb.Append("}"); // Fecha next_encounter_restrictions

            // Opcional: rollbackOnSuccess
            if (rollbackOnSuccess.HasValue)
            {
                string boolStr = rollbackOnSuccess.Value ? "true" : "false";
                sb.Append($",\"options\":{{\"rollback_on_success\":{boolStr}}}");
            }

            sb.Append("}"); // Fecha o JSON principal

            return sb.ToString();
        }

        private static void ProcessResponse(string encounterId, string json)
        {
            try
            {
                ApiResponse response = JsonUtility.FromJson<ApiResponse>(json);

                if (response.status != "success" || response.action_params == null)
                    throw new System.Exception("Unexpected response format.");

                Debug.Log(
                    $"[DDA] Model output received successfully (agent={response.agent}, action={response.action}).");

                DDAEventManager.Broadcast(new DDAModelOutputReceived(
                    response.action_params.mobile.count,
                    response.action_params.turret.count,
                    response.action_params.mobile.health,
                    response.action_params.mobile.hitbox,
                    response.action_params.turret.health,
                    response.action_params.turret.hitbox,
                    response.agent,
                    response.action
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