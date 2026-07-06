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
        private int m_defaultTotalEnemies = 1;

        private bool m_awaitingSpawn;

        private bool m_hasCachedPrediction;
        private int m_cachedTotalEnemies;

        private EncounterZone m_lastDdaZone;

        private string m_pendingDoorObstacleId;
        private string m_pendingDoorNextZoneId;

        public bool IsLastEncounter(string encounterId)
        {
            if (m_encounterZones.Count == 0) return false;
            return m_encounterZones[^1].Id.Equals(encounterId);
        }

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
                    .OrderBy(zone => zone.Id)
                    .ToList();

            string pendingRetryId = PlayerPrefs.GetString(DDAConfig.PENDING_RETRY_ENCOUNTER_ID, string.Empty);
            if (!string.IsNullOrEmpty(pendingRetryId))
            {
                PlayerPrefs.DeleteKey(DDAConfig.PENDING_RETRY_ENCOUNTER_ID);
                PlayerPrefs.Save();

                Debug.Log(
                    $"[EncounterZoneManager] Retry detectado para '{pendingRetryId}'. A pedir nova previsão à DDA.");
                m_awaitingSpawn = true;
                DDAService.Instance.NotifyEncounterCompleted(pendingRetryId);
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
            m_awaitingSpawn = !IsLastEncounter(evt.EncounterId);
        }

        private void OnModifiersUpdated(ModifiersUpdated evt)
        {
            if (!m_awaitingSpawn) return;

            TrySpawnNextZone(evt.TotalEnemies);
        }

        private void TrySpawnNextZone(int totalEnemies)
        {
            EncounterZone nextZone = GetNextEncounterZone();

            if (nextZone != null)
            {
                Debug.Log(
                    $"[EncounterZoneManager] Aplicando previsão da IA diretamente na próxima zona: {nextZone.Id}");

                nextZone.SpawnDDAEnemies(totalEnemies);
                m_lastDdaZone = nextZone;
                m_awaitingSpawn = false;
                m_hasCachedPrediction = false;
            }
            else
            {
                Debug.LogWarning(
                    "[EncounterZoneManager] Previsão recebida, mas nenhuma próxima zona ativa encontrada. Salvando em cache.");
                m_hasCachedPrediction = true;
                m_cachedTotalEnemies = totalEnemies;
            }
        }

        public void ForceSpawnEncounter(int totalEnemies)
        {
            EncounterZone targetZone = m_lastDdaZone != null ? m_lastDdaZone : GetNextEncounterZone();

            if (targetZone == null)
            {
                Debug.LogWarning("[EncounterZoneManager] ForceSpawnPrediction: nenhuma zona disponível para aplicar.");
                return;
            }

            Debug.Log($"[EncounterZoneManager] (DEBUG) Forçando respawn na zona: {targetZone.Id}");

            targetZone.SpawnDDAEnemies(totalEnemies);
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
                    $"[EncounterZoneManager] QueueDoorUnblock: nenhuma próxima zona encontrada. Abrindo porta '{trimmedId}' de imediato.");
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

            int totalEnemies = DDAModifierState.HasReceivedOutput
                ? DDAModifierState.TotalEnemiesModifier.Value
                : m_defaultTotalEnemies;

            Debug.LogWarning($"[EncounterZoneManager] Previsão falhou para '{evt.EncounterId}'. " +
                             $"Usando {(DDAModifierState.HasReceivedOutput ? "o último valor conhecido" : "o encontro por defeito")} ({totalEnemies} inimigos).");

            EncounterZone targetZone = m_lastDdaZone != null ? m_lastDdaZone : GetNextEncounterZone();
            if (targetZone != null)
            {
                targetZone.SpawnDDAEnemies(totalEnemies);
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

        /// <summary>
        /// Chamado se precisares de notificar o Manager que uma nova zona foi carregada dinamicamente.
        /// </summary>
        public void RegisterZone(EncounterZone newZone)
        {
            if (m_encounterZones.Contains(newZone)) return;

            m_encounterZones.Add(newZone);

            if (m_hasCachedPrediction)
            {
                newZone.SpawnDDAEnemies(m_cachedTotalEnemies);
                m_hasCachedPrediction = false;
                m_awaitingSpawn = false;
            }
        }

        private IEnumerator ApplyInitialEncounterState()
        {
            yield return null; // garante que todos os EncounterZone.Start() já correram

            if (m_awaitingSpawn) yield break; // retry pendente — a resposta da API trata disto via OnModifiersUpdated

            EncounterZone firstZone = GetNextEncounterZone();
            if (firstZone == null) yield break;

            if (DDAModifierState.HasReceivedOutput)
            {
                Debug.Log(
                    $"[EncounterZoneManager] Dados da DDA já disponíveis ao entrar nesta cena — aplicando à primeira zona: {firstZone.Id}");
                firstZone.SpawnDDAEnemies(DDAModifierState.TotalEnemiesModifier.Value);
            }
            else
            {
                Debug.Log(
                    $"[EncounterZoneManager] Nenhum dado da DDA disponível ainda — spawnando encontro por defeito em: {firstZone.Id}");
                firstZone.SpawnDDAEnemies(m_defaultTotalEnemies);
                m_lastDdaZone = firstZone;
            }
        }
    }
}