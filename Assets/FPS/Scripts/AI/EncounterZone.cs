using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [Header("Restrições DDA")] [SerializeField]
        private DDAEncounterRestrictionsSO m_restrictions;

        public DDAEncounterRestrictionsSO Restrictions => m_restrictions;

        private const int k_layerMask = 1 << EncounterZoneCollider.Layer;

        private ObjectiveKillEnemies m_currentObjective;
        private EnemyManager m_enemyManager;

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
            TotalMobilesDDA totalMobilesModifier = DDAModifierState.TotalMobilesModifier;
            if (DDAModifierState.HasReceivedOutput && totalMobilesModifier.Value < m_enemies.Count)
                m_enemies = m_enemies.Take(totalMobilesModifier.Value).ToList();

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
            m_enemyManager = FindAnyObjectByType<EnemyManager>();

            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
            EventManager.AddListener<EnemyDetectPlayerEvent>(OnPlayerDetected);
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

            PlayerPrefs.SetString(DDAConfig.K_PENDING_RETRY_ENCOUNTER_ID, Id);
            PlayerPrefs.Save();

            TrackEncounterEnded(EncounterEndReason.PlayerDied);

            _ = TelemetryService
                .FlushAsync(); //TODO: verificar vericidade, fire-and-forget — há tempo de sobra até o jogador clicar em retry
        }

        private IEnumerator TrackEncounterEndedByCompletion()
        {
            yield return null;
            TrackEncounterEnded(EncounterEndReason.Completed);

            //TODO: Verificar qualidade deste metodo
            Task flushTask = TelemetryService.FlushAsync();
            yield return new WaitUntil(() => flushTask.IsCompleted);

            DDAService.Instance.NotifyEncounterCompleted(Id);
            EncounterZoneManager.Instance?.QueueDoorUnblock(Id, m_obstacleIdToUnblock);
            DestroyEncounter();
        }

        public void SpawnDDAEnemies(int totalMobiles, int totalBosses)
        {
            foreach (GameObject enemy in m_enemies)
            {
                if (enemy == null) continue;

                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null && m_enemyManager != null)
                    m_enemyManager.UnregisterEnemySilently(enemyController);

                Destroy(enemy);
            }

            m_enemies.Clear();
            m_enemieIds.Clear();

            int bossesToSpawn =
                Mathf.Min(totalBosses, m_bossSpawnPoints.Count); // limite físico de spawn points, não de dificuldade
            for (int i = 0; i < bossesToSpawn; i++)
            {
                Transform spawnPoint = m_bossSpawnPoints[i];
                SpawnEnemy(m_bossEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
            }

            List<Transform> availableSpawnPoints = new(m_spawnPoints);
            for (int i = 0; i < totalMobiles; i++)
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

                    position = NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas)
                        ? hit.position
                        : reference.position;
                    rotation = reference.rotation;
                }

                SpawnEnemy(m_standardEnemyPrefab, position, rotation);
            }

            TotalEnemies = m_enemies.Count;

            if (m_currentObjective != null) m_currentObjective.ForceCompletion();
            m_currentObjective = gameObject.AddComponent<ObjectiveKillEnemies>();
            m_currentObjective.Title = "Eliminate all enemies";
            m_currentObjective.Description = "Defeat all the enemies in this encounter";
            m_currentObjective.MustKillAllEnemies = true;
            m_currentObjective.IsOptional = false;
            m_currentObjective.DelayVisible = 1;

            Debug.Log(
                $"[EncounterZone] {Id} — DDA spawned {TotalEnemies} enemies ({bossesToSpawn} bosses, {totalMobiles} standard).");
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