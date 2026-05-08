using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Telemetry;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    public class EncounterZone : MonoBehaviour
    {
        [SerializeField] private string m_id;

        [SerializeField] private List<GameObject> m_enemies;

        [SerializeField] private MapLocationSO m_location;

        [SerializeField] private string m_obstacleIdToUnblock;

        public bool HasStarted { get; private set; }
        public int TotalEnemies { get; private set; }
        public int RemainingEnemies => m_enemies.Count;

        private ActorsManager m_actorsManager;

        private void Awake()
        {
            TotalEnemies = m_enemies.Count;
        }

        private void Start()
        {
            for (int i = 0; i < m_enemies.Count; i++)
            {
                int index = i;
                Health enemyHealth = m_enemies[i].GetComponent<Health>();
                if (enemyHealth == null)
                {
                    continue;
                }

                UnityAction<GameObject> onEnemyDie = null;
                onEnemyDie = instigator =>
                {
                    enemyHealth.OnDamaged -= OnEnemyDamaged;
                    enemyHealth.OnDie -= onEnemyDie;
                };

                enemyHealth.OnDamaged += OnEnemyDamaged;
                enemyHealth.OnDie += onEnemyDie;
            }

            m_actorsManager = FindAnyObjectByType<ActorsManager>();
            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
            EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
        }

        private void OnTriggerEnter(Collider other)
        {
            Instigator instigator = other.GetComponent<Instigator>();
            if (instigator == null || instigator.Type != InstigatorType.Player)
            {
                return;
            }

            if (!HasStarted)
            {
                TrackEncounterStarted(EncounterStartReason.Entered);
                HasStarted = true;
            }

            EncounterZoneEnteredTelemetryData zoneEnteredTelemetryData = new(m_id, other.transform.position);
            TelemetryService.TrackEncounterZoneEnter(SessionManager.Instance.SessionID.ToString(),
                zoneEnteredTelemetryData);
        }

        private void OnTriggerExit(Collider other)
        {
            Instigator instigator = other.GetComponent<Instigator>();
            if (instigator == null || instigator.Type != InstigatorType.Player)
            {
                return;
            }

            EncounterZoneLeftTelemetryData telemetryData = new(m_id, other.transform.position);
            TelemetryService.TrackEncounterZoneLeave(SessionManager.Instance.SessionID.ToString(),
                telemetryData);
        }

        private void OnEnemyDamaged(float damage, GameObject damageSource)
        {
            if (HasStarted)
            {
                return;
            }

            Instigator instigator = damageSource.GetComponent<Instigator>();
            if (instigator == null || instigator.Type != InstigatorType.Player)
            {
                return;
            }

            TrackEncounterStarted(EncounterStartReason.AttackedEnemy);
            HasStarted = true;
        }

        private void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (!m_enemies.Remove(evt.Enemy))
            {
                return;
            }

            if (m_enemies.Count != 0)
            {
                return;
            }

            // Needed because player attack telemetry event ran after the encounter ended telemetry event
            StartCoroutine(TrackEncounterEndedByCompletion());
        }

        private void OnPlayerDeath(PlayerDeathEvent evt)
        {
            if (!HasStarted)
            {
                return;
            }

            TrackEncounterEnded(EncounterEndReason.PlayerDied);
        }

        private IEnumerator TrackEncounterEndedByCompletion()
        {
            yield return null;

            TrackEncounterEnded(EncounterEndReason.Completed);

            string id = m_obstacleIdToUnblock.Trim();
            if (id.Equals(string.Empty))
            {
                yield break;
            }

            ObstacleUnblockEvent evt = Events.ObstacleUnblockEvent;
            evt.ObstacleId = id;
            EventManager.Broadcast(evt);
        }

        private void TrackEncounterStarted(EncounterStartReason reason)
        {
            GameObject player = m_actorsManager?.Player;
            Health playerHealth = player?.GetComponent<Health>();
            PlayerWeaponsManager playerWeaponsManager = player?.GetComponent<PlayerWeaponsManager>();

            float playerStartHealth = playerHealth != null ? playerHealth.CurrentHealth : -1f;
            float playerStartAmmo = playerWeaponsManager != null ? playerWeaponsManager.GetTotalAmmo() : -1f;

            EncounterStartedTelemetryData startedTelemetryData = new(m_id, reason,
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

            EncounterEndedTelemetryData telemetryData = new(m_id, reason, playerRemainingHealth,
                playerRemainingAmmo, RemainingEnemies);
            TelemetryService.TrackEncounterEnd(SessionManager.Instance.SessionID.ToString(), telemetryData);
        }
    }
}