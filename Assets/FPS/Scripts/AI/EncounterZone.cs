using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.FPS.AI;
using Unity.FPS.Game;
using Unity.FPS.Telemetry;
using Unity.FPS.DDA;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    public class EncounterZone : MonoBehaviour
    {
        [SerializeField] private List<EncounterZoneCollider> m_zoneColliders = new();
        [SerializeField] public string Id;
        [SerializeField] private List<GameObject> m_enemies;
        [SerializeField] private List<PatrolPath> m_patrolPaths = new();
        [SerializeField] private MapLocationSO m_location;
        [SerializeField] private string m_obstacleIdToUnblock;

        [SerializeField] [Tooltip("The prefab used for standard enemies (HoverBot)")]
        private GameObject m_standardEnemyPrefab;

        [SerializeField] [Tooltip("The prefab used for boss enemies (Turret). Leave empty to disallow boss spawning.")]
        private GameObject m_bossEnemyPrefab;

        [SerializeField] [Tooltip("All available spawn points for this encounter.")]
        private List<Transform> m_spawnPoints = new();

        [SerializeField] [Tooltip("Boss-only spawn points (01-005: 1 point, 01-010: 2 points).")]
        private List<Transform> m_bossSpawnPoints = new();

        [SerializeField] [Tooltip("Maximum number of bosses to spawn. Only relevant if BossEnemyPrefab is assigned.")]
        private int m_maxBosses = 0;

        private const int k_layerMask = 1 << EncounterZoneCollider.Layer;

        public bool HasStarted { get; private set; }
        public int TotalEnemies { get; private set; }
        public int RemainingEnemies => m_enemies.Count;

        private ActorsManager m_actorsManager;
        private bool m_isPlayerInZone;
        private GameObject m_lastEnteredCollider;

        private readonly List<int> m_enemieIds = new();

        private void Awake()
        {
            TotalEnemies = m_enemies.Count;
        }

        private void Start()
        {
            // DDA: override enemy count if a model output has been received
            TotalEnemiesDDA totalEnemiesModifier = DDAModifierState.TotalEnemiesModifier;
            if (DDAModifierState.HasReceivedOutput && totalEnemiesModifier.Value < m_enemies.Count)
                m_enemies = m_enemies.Take(totalEnemiesModifier.Value).ToList();

            TotalEnemies = m_enemies.Count;

            for (int i = 0; i < m_enemies.Count; i++)
            {
                Instigator instigator = m_enemies[i].GetComponent<Instigator>();
                if (instigator == null) continue;

                EnemyController enemyController = m_enemies[i].GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    PatrolPath nearestPath = GetNearestPatrolPath(m_enemies[i].transform.position);
                    if (nearestPath != null) enemyController.PatrolPath = nearestPath;
                }

                Health enemyHealth = m_enemies[i].GetComponent<Health>();
                if (enemyHealth == null) continue;


                UnityAction<GameObject> onEnemyDie = null;
                onEnemyDie = instigator =>
                {
                    enemyHealth.OnDamaged -= OnEnemyDamaged;
                    enemyHealth.OnDie -= onEnemyDie;
                };

                enemyHealth.OnDamaged += OnEnemyDamaged;
                enemyHealth.OnDie += onEnemyDie;
            }

            foreach (EncounterZoneCollider zoneCollider in m_zoneColliders)
            {
                zoneCollider.OnPlayerEntered += OnPlayerEnter;
                zoneCollider.OnPlayerExited += OnPlayerExit;
            }

            m_actorsManager = FindAnyObjectByType<ActorsManager>();

            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
            EventManager.AddListener<EnemyDetectPlayerEvent>(OnPlayerDetected);

            DDAEventManager.Broadcast(new EncounterEnemiesSpawnedEvent(Id));
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
            EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
            EventManager.RemoveListener<EnemyDetectPlayerEvent>(OnPlayerDetected);
        }

        private void OnPlayerEnter(Instigator player, GameObject colliderEntered)
        {
            if (!HasStarted)
            {
                TrackEncounterStarted(EncounterStartReason.Entered);
                HasStarted = true;
            }

            if (!m_isPlayerInZone)
            {
                EncounterZoneEnteredTelemetryData zoneEnteredTelemetryData = new(Id, player.transform.position);
                TelemetryService.TrackEncounterZoneEnter(SessionManager.Instance.SessionID.ToString(),
                    zoneEnteredTelemetryData);
                m_isPlayerInZone = true;
            }

            m_lastEnteredCollider = colliderEntered;
        }

        private void OnPlayerExit(Instigator player, GameObject colliderLeft)
        {
            // Overlapping collider
            if (m_lastEnteredCollider != colliderLeft) return;

            Collider[] hitColliders = Physics.OverlapBox(player.transform.position,
                player.transform.localScale / 2, Quaternion.identity, k_layerMask);

            foreach (Collider encounterCollider in hitColliders)
            {
                // Non-overlapping neighbor collider
                if (encounterCollider.gameObject != m_lastEnteredCollider) return;
            }

            EncounterZoneLeftTelemetryData telemetryData = new(Id, player.transform.position);
            TelemetryService.TrackEncounterZoneLeave(SessionManager.Instance.SessionID.ToString(),
                telemetryData);
            m_isPlayerInZone = false;
        }

        private void OnPlayerDetected(EnemyDetectPlayerEvent evt)
        {
            if (HasStarted || !m_enemieIds.Contains(evt.InstigatorId)) return;

            TrackEncounterStarted(EncounterStartReason.Detected);
            HasStarted = true;
        }

        private void OnEnemyDamaged(float damage, GameObject damageSource)
        {
            if (HasStarted) return;

            Instigator instigator = damageSource.GetComponent<Instigator>();
            if (instigator == null || instigator.Type != InstigatorType.Player) return;

            TrackEncounterStarted(EncounterStartReason.AttackedEnemy);
            HasStarted = true;
        }

        private void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (!m_enemies.Remove(evt.Enemy)) return;
            if (m_enemies.Count != 0) return;

            StartCoroutine(TrackEncounterEndedByCompletion());
        }

        private void OnPlayerDeath(PlayerDeathEvent evt)
        {
            if (!HasStarted) return;

            PlayerPrefs.SetString(DDAConfig.PENDING_RETRY_ENCOUNTER_ID, Id);
            PlayerPrefs.Save();

            TrackEncounterEnded(EncounterEndReason.PlayerDied);
        }

        private IEnumerator TrackEncounterEndedByCompletion()
        {
            yield return null;

            TrackEncounterEnded(EncounterEndReason.Completed);
            DDAService.Instance.NotifyEncounterCompleted(Id);
            EncounterZoneManager.Instance?.QueueDoorUnblock(Id, m_obstacleIdToUnblock);
            DestroyEncounter();
        }

        public void SpawnDDAEnemies(int totalEnemies)
        {
            foreach (GameObject enemy in m_enemies)
            {
                Destroy(enemy);
            }

            m_enemies.Clear();
            m_enemieIds.Clear();

            bool canSpawnBoss = m_bossEnemyPrefab != null && m_bossSpawnPoints.Count > 0;
            int totalRequested = totalEnemies;

            int bossCount = 0;
            int bossSpawned = 0;

            if (canSpawnBoss)
            {
                int maxPossibleBosses = Mathf.Min(m_maxBosses, m_bossSpawnPoints.Count);
                bossCount = Mathf.Min(totalRequested, maxPossibleBosses);

                for (int i = 0; i < bossCount; i++)
                {
                    if (Random.value > 0.5f) continue;
                    bossSpawned++;
                    Transform spawnPoint = m_bossSpawnPoints[i];
                    SpawnEnemy(m_bossEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
                }
            }

            int standardCount = totalRequested - bossSpawned;
            List<Transform> availableSpawnPoints = new(m_spawnPoints);

            for (int i = 0; i < standardCount; i++)
            {
                Vector3 position;
                Quaternion rotation;

                if (availableSpawnPoints.Count > 0)
                {
                    int index = Random.Range(0, availableSpawnPoints.Count);
                    position = availableSpawnPoints[index].position;
                    rotation = availableSpawnPoints[index].rotation;
                    availableSpawnPoints.RemoveAt(index);
                }
                else
                {
                    Transform reference = m_spawnPoints[Random.Range(0, m_spawnPoints.Count)];
                    Vector2 offset = Random.insideUnitCircle * 2f;
                    Vector3 candidate = reference.position + new Vector3(offset.x, 0f, offset.y);

                    if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                        position = hit.position;
                    else
                        position = reference.position;

                    rotation = reference.rotation;
                }

                SpawnEnemy(m_standardEnemyPrefab, position, rotation);
            }

            TotalEnemies = m_enemies.Count;

            Debug.Log(
                $"[EncounterZone] {Id} — DDA spawned {TotalEnemies} enemies ({bossCount} bosses, {TotalEnemies - bossCount} standard).");

            DDAEventManager.Broadcast(new EncounterEnemiesSpawnedEvent(Id));
        }

        private void SpawnEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return;

            GameObject spawned = Instantiate(prefab, position, rotation);

            EnemyController enemyController = spawned.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                PatrolPath nearestPath = GetNearestPatrolPath(position);
                if (nearestPath != null) enemyController.PatrolPath = nearestPath;
            }

            m_enemies.Add(spawned);
            RegisterEnemyDeathListener(spawned);
        }

        private void RegisterEnemyDeathListener(GameObject enemy)
        {
            Instigator instigator = enemy.GetComponent<Instigator>();
            Health enemyHealth = enemy.GetComponent<Health>();
            if (instigator == null || enemyHealth == null) return;

            m_enemieIds.Add(instigator.Id);

            UnityAction<GameObject> onEnemyDie = null;
            onEnemyDie = _ =>
            {
                enemyHealth.OnDamaged -= OnEnemyDamaged;
                enemyHealth.OnDie -= onEnemyDie;
            };

            enemyHealth.OnDamaged += OnEnemyDamaged;
            enemyHealth.OnDie += onEnemyDie;
        }

        private void TrackEncounterStarted(EncounterStartReason reason)
        {
            GameObject player = m_actorsManager?.Player;
            Health playerHealth = player?.GetComponent<Health>();
            PlayerWeaponsManager playerWeaponsManager = player?.GetComponent<PlayerWeaponsManager>();

            float playerStartHealth = playerHealth != null ? playerHealth.CurrentHealth : -1f;
            float playerStartAmmo = playerWeaponsManager != null ? playerWeaponsManager.GetTotalAmmo() : -1f;

            EncounterStartedTelemetryData startedTelemetryData = new(Id, reason,
                TotalEnemies, playerStartHealth, playerStartAmmo, m_location);
            TelemetryService.TrackEncounterStart(SessionManager.Instance.SessionID.ToString(),
                startedTelemetryData);
        }

        private void TrackEncounterEnded(EncounterEndReason reason)
        {
            GameObject player = m_actorsManager?.Player;
            Health playerHealth = player?.GetComponent<Health>();
            PlayerWeaponsManager playerWeaponsManager = player?.GetComponent<PlayerWeaponsManager>();

            float playerRemainingHealth = playerHealth != null ? playerHealth.CurrentHealth : -1f;
            float playerRemainingAmmo = playerWeaponsManager != null ? playerWeaponsManager.GetTotalAmmo() : -1f;

            EncounterEndedTelemetryData telemetryData = new(Id, reason, playerRemainingHealth,
                playerRemainingAmmo, RemainingEnemies);
            TelemetryService.TrackEncounterEnd(SessionManager.Instance.SessionID.ToString(), telemetryData);
        }

        private PatrolPath GetNearestPatrolPath(Vector3 position)
        {
            PatrolPath nearest = null;
            float nearestDist = float.MaxValue;

            foreach (PatrolPath path in m_patrolPaths)
            {
                if (path == null) continue;

                float dist = Vector3.Distance(position, path.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = path;
                }
            }

            return nearest;
        }

        private void DestroyEncounter()
        {
            foreach (EncounterZoneCollider collider in m_zoneColliders)
            {
                Destroy(collider.gameObject);
            }

            Destroy(gameObject);
        }
    }
}