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

        private EncounterZone m_nextZone;

        [Header("Porta de Entrada (Encounter 1)")]
        [Tooltip(
            "ID do obstáculo/porta a abrir assim que o primeiro encontro desta cena ficar pronto (spawnado). Deixa vazio se esta cena não tiver porta de entrada.")]
        [SerializeField]
        private string m_entryObstacleId;

        private bool m_entryDoorOpened;

        [Header("Fallback (sem dados da DDA)")]
        [Tooltip(
            "Usado APENAS no primeiro encontro do jogo, quando ainda não existe nenhuma restrição previamente utilizada pela DDA (DDAModifierState.LastUsedRestrictions). Em qualquer falha posterior (sem próxima zona, API indisponível, etc.), são reutilizados os últimos valores efetivamente usados.")]
        [SerializeField]
        private DDAEncounterRestrictionsSO m_firstEncounterDefaults;

        private bool m_awaitingSpawn;

        private bool m_hasCachedPrediction;

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
            
            DDAEventManager.AddListener<EncounterCompleted>(OnEncounterCompleted);
            DDAEventManager.AddListener<ModifiersUpdated>(OnModifiersUpdated);
            DDAEventManager.AddListener<DDAPredictionFailed>(OnPredictionFailed);
            DDAEventManager.AddListener<EncounterEnemiesSpawnedEvent>(OnZoneEnemiesSpawned);

            Instance = this;

            if (m_encounterZones.Count == 0)
                m_encounterZones = FindObjectsByType<EncounterZone>(FindObjectsSortMode.None)
                    .OrderBy(zone => zone.Id).ToList();
            
        }

        private void Start()
        {
            m_nextZone = GetNextEncounterZone();
            if (m_nextZone == null) return;

            string pendingRetryId = PlayerPrefs.GetString(DDAConfig.K_PENDING_RETRY_ENCOUNTER_ID, string.Empty);
            if (!string.IsNullOrEmpty(pendingRetryId))
            {
                PlayerPrefs.DeleteKey(DDAConfig.K_PENDING_RETRY_ENCOUNTER_ID);
                PlayerPrefs.Save();

                Debug.Log($"[EncounterZoneManager] Retry detectado para '{pendingRetryId}'.");

                m_nextZone = GetNextEncounterZone();
                DDAEncounterRestrictionsSO nextRestrictions =
                    m_nextZone != null ? m_nextZone.Restrictions : GetFallbackRestrictions();

                m_awaitingSpawn = true;
                DDAController.RequestPrediction(pendingRetryId, nextRestrictions);
                return;
            }

            string pendingLevelEncounterId =
                PlayerPrefs.GetString(DDAConfig.K_PENDING_LEVEL_TRANSITION_ENCOUNTER_ID, string.Empty);
            if (!string.IsNullOrEmpty(pendingLevelEncounterId))
            {
                PlayerPrefs.DeleteKey(DDAConfig.K_PENDING_LEVEL_TRANSITION_ENCOUNTER_ID);
                PlayerPrefs.Save();


                m_nextZone = GetNextEncounterZone();

                DDAEncounterRestrictionsSO nextRestrictionsSo =
                    m_nextZone != null ? m_nextZone.Restrictions : GetFallbackRestrictions();

                Debug.Log(
                    $"[EncounterZoneManager] Transição de nível detectada. A pedir previsão à DDA com base em '{pendingLevelEncounterId}'.");

                m_awaitingSpawn = true;
                DDAController.RequestPrediction(pendingLevelEncounterId, nextRestrictionsSo);
                return;
            }
            
            Debug.Log(
                $"[EncounterZoneManager] Nenhum pedido pendente — spawnando encontro por defeito em: {m_nextZone.Id}");
            m_nextZone.SpawnDDAEnemies(DDAModifierState.TotalMobilesModifier.Value,
                DDAModifierState.TotalTurretsModifier.Value);

            m_lastDdaZone = m_nextZone;
        }

        private void OnDestroy()
        {
            DDAEventManager.RemoveListener<EncounterCompleted>(OnEncounterCompleted);
            DDAEventManager.RemoveListener<ModifiersUpdated>(OnModifiersUpdated);
            DDAEventManager.RemoveListener<DDAPredictionFailed>(OnPredictionFailed);
            DDAEventManager.RemoveListener<EncounterEnemiesSpawnedEvent>(OnZoneEnemiesSpawned);
        }

        public EncounterZone GetNextEncounterZone()
        {
            return m_encounterZones.FirstOrDefault(zone => zone != null && !zone.HasStarted);
        }

        private DDAEncounterRestrictionsSO GetFallbackRestrictions()
        {
            return DDAModifierState.LastUsedRestrictions != null
                ? DDAModifierState.LastUsedRestrictions
                : m_firstEncounterDefaults;
        }

        private void OnEncounterCompleted(EncounterCompleted evt)
        {
            if (IsLastEncounter(evt.EncounterId))
            {
                Debug.Log(
                    $"[EncounterZoneManager] Último encontro do nível ('{evt.EncounterId}'). A previsão será pedida na próxima cena.");

                PlayerPrefs.SetString(DDAConfig.K_PENDING_LEVEL_TRANSITION_ENCOUNTER_ID, evt.EncounterId);
                PlayerPrefs.Save();

                m_awaitingSpawn = false;
                return;
            }

            m_awaitingSpawn = true;

            m_nextZone = GetNextEncounterZone();
            DDAEncounterRestrictionsSO nextRestrictions =
                m_nextZone != null ? m_nextZone.Restrictions : GetFallbackRestrictions();

            DDAController.RequestPrediction(evt.EncounterId, nextRestrictions);
        }

        private void OnModifiersUpdated(ModifiersUpdated evt)
        {
            if (!m_awaitingSpawn) return;
            TrySpawnNextZone(evt.TotalMobiles, evt.TotalBosses);
        }

        private void TrySpawnNextZone(int totalMobiles, int totalBosses)
        {
            if (m_nextZone != null)
            {
                m_nextZone.SpawnDDAEnemies(totalMobiles, totalBosses);
                m_lastDdaZone = m_nextZone;
                m_awaitingSpawn = false;
                m_hasCachedPrediction = false;
            }
            else
            {
                m_hasCachedPrediction = true;
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

            if (m_nextZone == null)
            {
                Debug.LogWarning(
                    $"[EncounterZoneManager] Nenhuma próxima zona encontrada. Abrindo porta '{trimmedId}' de imediato.");
                OpenDoor(trimmedId);
                return;
            }

            Debug.Log($"[EncounterZoneManager] Porta '{trimmedId}' à espera do spawn de '{m_nextZone.Id}'.");
            m_pendingDoorObstacleId = trimmedId;
            m_pendingDoorNextZoneId = m_nextZone.Id;
        }

        private void OnZoneEnemiesSpawned(EncounterEnemiesSpawnedEvent evt)
        {
            TryOpenEntryDoor(evt.EncounterId);

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

        private void TryOpenEntryDoor(string readyEncounterId)
        {
            if (m_entryDoorOpened) return;
            if (string.IsNullOrEmpty(m_entryObstacleId)) return;
            if (m_encounterZones.Count == 0 || m_encounterZones[0] == null) return;
            if (m_encounterZones[0].Id != readyEncounterId) return;

            Debug.Log(
                $"[EncounterZoneManager] '{readyEncounterId}' (primeiro encontro) pronto — abrindo porta de entrada '{m_entryObstacleId}'.");
            OpenDoor(m_entryObstacleId);
            m_entryDoorOpened = true;
        }

        private void OnPredictionFailed(DDAPredictionFailed evt)
        {
            if (!m_awaitingSpawn && m_pendingDoorObstacleId == null) return;

            EncounterZone targetZone = m_lastDdaZone != null ? m_lastDdaZone : m_nextZone;
            int fallbackTotalMobile = DDAModifierState.TotalMobilesModifier.Value;
            int fallbackTotalBoss = DDAModifierState.TotalTurretsModifier.Value;

            Debug.LogWarning(
                $"[EncounterZoneManager] Previsão falhou para '{m_nextZone.Id}'. Usando {fallbackTotalMobile} inimigos.");

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

        public bool IsLastEncounter(string encounterId)
        {
            if (m_encounterZones.Count == 0) return false;
            return m_encounterZones[^1].Id.Equals(encounterId);
        }
    }
}