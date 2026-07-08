using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.FPS.DDA;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    // TODO: Add o DDAService.Instance.NotifyEncounterCompleted(Id); e EncounterZoneManager.Instance?.QueueDoorUnblock(Id, m_obstacleIdToUnblock);
    // Para passar os limites
    // Adicionar o evento da telemetria para impedir o DDA ir ler antes da Telemetry acabar (bool)
    public class EncounterZoneManager : MonoBehaviour
    {
        public static EncounterZoneManager Instance { get; private set; }

        [Header("Zonas de Encontro")] [SerializeField]
        private List<EncounterZone> m_encounterZones = new();

        [Header("Fallback (sem dados da DDA)")]
        [Tooltip(
            "Número de inimigos usado quando ainda não há nenhuma previsão da DDA disponível (primeira vez a jogar, ou falha da API). Vida/hitbox usam sempre o valor base do prefab nestes casos.")]
        [SerializeField]
        private bool m_awaitingSpawn;

        private bool m_startupRequestIssued;

        private bool m_hasCachedPrediction;
        private int m_cachedTotalMobiles;
        private int m_cachedTotalBosses;

        private EncounterZone m_lastDdaZone;

        private string m_pendingDoorObstacleId;
        private string m_pendingDoorNextZoneId;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (m_encounterZones.Count == 0)
                m_encounterZones = FindObjectsByType<EncounterZone>(FindObjectsSortMode.None)
                    .OrderBy(zone => zone.Id).ToList();

            string pendingRetryId = PlayerPrefs.GetString(DDAConfig.K_PENDING_RETRY_ENCOUNTER_ID, string.Empty);
            if (!string.IsNullOrEmpty(pendingRetryId))
            {
                PlayerPrefs.DeleteKey(DDAConfig.K_PENDING_RETRY_ENCOUNTER_ID);
                PlayerPrefs.Save();

                Debug.Log($"[EncounterZoneManager] Retry detectado para '{pendingRetryId}'.");

                EncounterZone nextZone = GetNextEncounterZone();
                DDAEncounterRestrictions nextRestrictions =
                    nextZone != null ? nextZone.Restrictions : DDAEncounterRestrictions.GlobalFallback;

                m_awaitingSpawn = true;
                m_startupRequestIssued = true;
                DDAController.RequestPrediction(pendingRetryId, nextRestrictions);
                return;
            }

            string pendingLevelEncounterId =
                PlayerPrefs.GetString(DDAConfig.K_PENDING_LEVEL_TRANSITION_ENCOUNTER_ID, string.Empty);
            if (!string.IsNullOrEmpty(pendingLevelEncounterId))
            {
                PlayerPrefs.DeleteKey(DDAConfig.K_PENDING_LEVEL_TRANSITION_ENCOUNTER_ID);
                PlayerPrefs.DeleteKey(DDAConfig.K_PENDING_LEVEL_TRANSITION_RESTRICTIONS);
                PlayerPrefs.Save();


                EncounterZone nextZone = GetNextEncounterZone();
                DDAEncounterRestrictions nextRestrictions =
                    nextZone != null ? nextZone.Restrictions : DDAEncounterRestrictions.GlobalFallback;

                Debug.Log(
                    $"[EncounterZoneManager] Transição de nível detectada. A pedir previsão à DDA com base em '{pendingLevelEncounterId}'.");

                m_awaitingSpawn = true;
                m_startupRequestIssued = true;
                DDAController.RequestPrediction(pendingLevelEncounterId, nextRestrictions);
            }
        }

        private void Start()
        {
            StartCoroutine(ApplyInitialEncounterState());
        }

        private void OnEnable()
        {
            DDAEventManager.AddListener<EncounterCompleted>(OnEncounterCompleted);
            DDAEventManager.AddListener<ModifiersUpdated>(OnModifiersUpdated);
            DDAEventManager.AddListener<DDAPredictionFailed>(OnPredictionFailed);
            DDAEventManager.AddListener<EncounterEnemiesSpawnedEvent>(OnZoneEnemiesSpawned);
        }

        private void OnDisable()
        {
            DDAEventManager.RemoveListener<EncounterCompleted>(OnEncounterCompleted);
            DDAEventManager.RemoveListener<ModifiersUpdated>(OnModifiersUpdated);
            DDAEventManager.RemoveListener<DDAPredictionFailed>(OnPredictionFailed);
            DDAEventManager.RemoveListener<EncounterEnemiesSpawnedEvent>(OnZoneEnemiesSpawned);
        }

        /// <summary>
        /// Retorna a próxima zona de encontro que ainda não foi iniciada.
        /// </summary>
        public EncounterZone GetNextEncounterZone()
        {
            return m_encounterZones.FirstOrDefault(zone => zone != null && !zone.HasStarted);
        }

        private void OnEncounterCompleted(EncounterCompleted evt)
        {
            EncounterZone completedZone = m_encounterZones.FirstOrDefault(z => z != null && z.Id == evt.EncounterId);
            DDAEncounterRestrictions previousRestrictions = completedZone != null
                ? completedZone.Restrictions
                : DDAEncounterRestrictions.GlobalFallback;

            if (IsLastEncounter(evt.EncounterId))
            {
                Debug.Log(
                    $"[EncounterZoneManager] Último encontro do nível ('{evt.EncounterId}'). A previsão será pedida na próxima cena.");

                PlayerPrefs.SetString(DDAConfig.K_PENDING_LEVEL_TRANSITION_ENCOUNTER_ID, evt.EncounterId);
                PlayerPrefs.SetString(DDAConfig.K_PENDING_LEVEL_TRANSITION_RESTRICTIONS,
                    JsonUtility.ToJson(previousRestrictions));
                PlayerPrefs.Save();

                m_awaitingSpawn = false;
                return;
            }

            m_awaitingSpawn = true;

            EncounterZone nextZone = GetNextEncounterZone();
            DDAEncounterRestrictions nextRestrictions =
                nextZone != null ? nextZone.Restrictions : DDAEncounterRestrictions.GlobalFallback;

            DDAController.RequestPrediction(evt.EncounterId, nextRestrictions);
        }

        private void OnModifiersUpdated(ModifiersUpdated evt)
        {
            if (!m_awaitingSpawn) return;
            TrySpawnNextZone(evt.TotalMobiles, evt.TotalBosses);
        }

        private void TrySpawnNextZone(int totalMobiles, int totalBosses)
        {
            EncounterZone nextZone = GetNextEncounterZone();
            if (nextZone != null)
            {
                nextZone.SpawnDDAEnemies(totalMobiles, totalBosses);
                m_lastDdaZone = nextZone;
                m_awaitingSpawn = false;
                m_hasCachedPrediction = false;
            }
            else
            {
                m_hasCachedPrediction = true;
                m_cachedTotalMobiles = totalMobiles;
                m_cachedTotalBosses = totalBosses;
            }
        }

        public void ForceSpawnEncounter(int totalMobiles, int totalBosses)
        {
            EncounterZone targetZone = m_lastDdaZone != null ? m_lastDdaZone : GetNextEncounterZone();
            if (targetZone == null) return;

            targetZone.SpawnDDAEnemies(totalMobiles, totalBosses);
            m_lastDdaZone = targetZone;
            m_awaitingSpawn = false;
            m_hasCachedPrediction = false;
        }

        public void QueueDoorUnblock(string completedEncounterId, string obstacleId)
        {
            string trimmedId = obstacleId?.Trim();
            if (string.IsNullOrEmpty(trimmedId)) return;

            if (IsLastEncounter(completedEncounterId))
            {
                Debug.Log(
                    $"[EncounterZoneManager] '{completedEncounterId}' é o último encontro — abrindo porta '{trimmedId}' de imediato.");
                OpenDoor(trimmedId);
                return;
            }

            EncounterZone nextZone = GetNextEncounterZone();
            if (nextZone == null)
            {
                Debug.LogWarning(
                    $"[EncounterZoneManager] Nenhuma próxima zona encontrada. Abrindo porta '{trimmedId}' de imediato.");
                OpenDoor(trimmedId);
                return;
            }

            Debug.Log($"[EncounterZoneManager] Porta '{trimmedId}' à espera do spawn de '{nextZone.Id}'.");
            m_pendingDoorObstacleId = trimmedId;
            m_pendingDoorNextZoneId = nextZone.Id;
        }

        private void OnZoneEnemiesSpawned(EncounterEnemiesSpawnedEvent evt)
        {
            if (m_pendingDoorObstacleId == null) return;
            if (evt.EncounterId != m_pendingDoorNextZoneId) return;

            Debug.Log(
                $"[EncounterZoneManager] '{evt.EncounterId}' pronta — abrindo porta '{m_pendingDoorObstacleId}'.");
            OpenDoor(m_pendingDoorObstacleId);

            m_pendingDoorObstacleId = null;
            m_pendingDoorNextZoneId = null;
        }

        private void OpenDoor(string obstacleId)
        {
            ObstacleUnblockEvent evt = Events.ObstacleUnblockEvent;
            evt.ObstacleId = obstacleId;
            EventManager.Broadcast(evt);
        }

        private void OnPredictionFailed(DDAPredictionFailed evt)
        {
            if (!m_awaitingSpawn && m_pendingDoorObstacleId == null) return;

            EncounterZone targetZone = m_lastDdaZone != null ? m_lastDdaZone : GetNextEncounterZone();
            int fallbackTotalMobile = DDAModifierState.TotalMobilesModifier.Value;
            int fallbackTotalBoss = DDAModifierState.TotalTurretsModifier.Value;

            Debug.LogWarning(
                $"[EncounterZoneManager] Previsão falhou para '{evt.EncounterId}'. Usando {fallbackTotalMobile} inimigos.");

            if (targetZone != null)
            {
                targetZone.SpawnDDAEnemies(fallbackTotalMobile, fallbackTotalBoss);
                m_lastDdaZone = targetZone;
            }

            m_awaitingSpawn = false;
            m_hasCachedPrediction = false;

            if (m_pendingDoorObstacleId != null)
            {
                OpenDoor(m_pendingDoorObstacleId);
                m_pendingDoorObstacleId = null;
                m_pendingDoorNextZoneId = null;
            }
        }

        private IEnumerator ApplyInitialEncounterState()
        {
            yield return null;

            if (m_startupRequestIssued) yield break;

            EncounterZone firstZone = GetNextEncounterZone();
            if (firstZone == null) yield break;

            Debug.Log(
                $"[EncounterZoneManager] Nenhum pedido pendente — spawnando encontro por defeito em: {firstZone.Id}");
            firstZone.SpawnDDAEnemies(DDAModifierState.TotalMobilesModifier.Value,
                DDAModifierState.TotalTurretsModifier.Value);

            m_lastDdaZone = firstZone;
        }

        public bool IsLastEncounter(string encounterId)
        {
            if (m_encounterZones.Count == 0) return false;
            return m_encounterZones[^1].Id.Equals(encounterId);
        }

        /// <summary>
        /// Chamado se precisares de notificar o Manager que uma nova zona foi carregada dinamicamente.
        /// </summary>
        public void RegisterZone(EncounterZone newZone)
        {
            if (m_encounterZones.Contains(newZone)) return;

            m_encounterZones.Add(newZone);

            if (m_hasCachedPrediction)
            {
                newZone.SpawnDDAEnemies(m_cachedTotalMobiles, m_cachedTotalBosses);
                m_hasCachedPrediction = false;
            }
        }
    }
}