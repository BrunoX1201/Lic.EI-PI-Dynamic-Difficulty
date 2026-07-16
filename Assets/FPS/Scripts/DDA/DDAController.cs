using System;
using System.Globalization;
using System.Threading.Tasks;
using Unity.FPS.API;
using UnityEngine;

namespace Unity.FPS.DDA
{
    public static class DDAController
    {
        public static void RequestPrediction(string encounterId, DDAEncounterRestrictionsSO nextEncounterRestrictions,
            bool? rollbackOnSuccess = null)
        {
            _ = SendRequest(encounterId, nextEncounterRestrictions, rollbackOnSuccess);
        }

        private static async Task SendRequest(string encounterId, DDAEncounterRestrictionsSO nextEncounterRestrictions,
            bool? rollbackOnSuccess)
        {
            ProcessEncounterRequest request = BuildRequest(encounterId, nextEncounterRestrictions, rollbackOnSuccess);

            Debug.Log($"[DDA] Requesting adjustment from encounter: {encounterId}");
            APIResult<ProcessEncounterResponse> result = await APIService.ProcessEncounter(request);

            if (!result.Success)
            {
                Debug.LogError($"[DDA] API error: {result.Error}. Default modifiers will be kept.");
                DDAEventManager.Broadcast(new DDAPredictionFailed());
                return;
            }

            ProcessEncounterResponse response = result.Value;
            Debug.Log(
                $"[DDA] Model output received successfully (agent={response.Agent}, action={response.Action}).");

            try
            {
                TextInfo ti = CultureInfo.InvariantCulture.TextInfo;
                string actionTitleCase = ti.ToTitleCase(response.Action.ToLower());
                actionTitleCase = actionTitleCase.Replace("_", string.Empty);

                Enum.TryParse(actionTitleCase, out DDADirection action);

                DDAEventManager.Broadcast(new DDAModelOutputReceived(
                    response.ActionParams.Mobile.Count,
                    response.ActionParams.Turret.Count,
                    response.ActionParams.Mobile.Health,
                    response.ActionParams.Mobile.Hitbox,
                    response.ActionParams.Turret.Health,
                    response.ActionParams.Turret.Hitbox,
                    response.Agent,
                    action));
            }
            catch (Exception e)
            {
                Debug.LogError($"[DDA] API error: {e.Message}. Default modifiers will be kept.");
                DDAEventManager.Broadcast(new DDAPredictionFailed());
            }
        }

        private static ProcessEncounterRequest BuildRequest(string encounterId, DDAEncounterRestrictionsSO next,
            bool? rollbackOnSuccess)
        {
            DDAModifierState.SetLastUsedRestrictions(next);
            ProcessEncounterRequest request = new()
            {
                EncounterId = encounterId,
                NextEncounterRestrictions = new EncounterRestrictions
                {
                    Turret = new EnemyTypeRestrictions
                    {
                        Count = new LimitValue<int>(next.MinBosses, next.MaxBosses, next.DefaultBosses,
                            DDAModifierState.TotalTurretsModifier.Value),
                        Health = new LimitValue<float>(next.MinTurretHealth, next.MaxTurretHealth,
                            next.DefaultTurretHealth, DDAModifierState.TurretHealthModifier.Value),
                        Hitbox = new LimitValue<float>(next.MinTurretHitbox, next.MaxTurretHitbox,
                            next.DefaultTurretHitbox, DDAModifierState.TurretHitboxModifier.Value)
                    },
                    Mobile = new EnemyTypeRestrictions
                    {
                        Count = new LimitValue<int>(next.MinMobiles, next.MaxMobiles, next.DefaultMobiles,
                            DDAModifierState.TotalMobilesModifier.Value),
                        Health = new LimitValue<float>(next.MinMobileHealth, next.MaxMobileHealth,
                            next.DefaultMobileHealth, DDAModifierState.MobileHealthModifier.Value),
                        Hitbox = new LimitValue<float>(next.MinMobileHitbox, next.MaxMobileHitbox,
                            next.DefaultMobileHitbox, DDAModifierState.MobileHitboxModifier.Value)
                    }
                }
            };

            if (rollbackOnSuccess.HasValue)
            {
                request.Options = new ProcessEncounterOptions { RollbackOnSuccess = rollbackOnSuccess.Value };
            }

            return request;
        }
    }
}