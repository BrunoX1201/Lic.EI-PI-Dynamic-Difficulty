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
        [SerializeField] private List<EncounterZoneCollider> m_zoneColliders = new();

        [SerializeField] private string m_id;

        [SerializeField] private List<GameObject> m_enemies;

        [SerializeField] private MapLocationSO m_location;
        private const int k_layerMask = 1 << EncounterZoneCollider.Layer;

        public bool HasStarted { get; private set; }
        public int TotalEnemies { get; private set; }
        public int RemainingEnemies => m_enemies.Count;

        private ActorsManager m_actorsManager;
        private bool m_isPlayerInZone;
        private GameObject m_lastEnteredCollider;

        private void Awake()
        {
            TotalEnemies = m_enemies.Count;
        }

        private void Start()
        {
            for (int i = 0; i < m_enemies.Count; i++)
            {
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

            foreach (EncounterZoneCollider zoneCollider in m_zoneColliders)
            {
                zoneCollider.OnPlayerEntered += OnPlayerEnter;
                zoneCollider.OnPlayerExited += OnPlayerExit;
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


        private void OnPlayerEnter(Instigator player, GameObject colliderEntered)
        {
            if (!HasStarted)
            {
                TrackEncounterStarted(EncounterStartReason.Entered);
                HasStarted = true;
            }

            if (!m_isPlayerInZone)
            {
                EncounterZoneEnteredTelemetryData zoneEnteredTelemetryData = new(m_id, player.transform.position);
                TelemetryService.TrackEncounterZoneEnter(SessionManager.Instance.SessionID.ToString(),
                    zoneEnteredTelemetryData);
                m_isPlayerInZone = true;
            }

            m_lastEnteredCollider = colliderEntered;
        }

        private void OnPlayerExit(Instigator player, GameObject colliderLeft)
        {
            // Overlapping collider
            if (m_lastEnteredCollider != colliderLeft)
            {
                return;
            }

            Collider[] hitColliders = Physics.OverlapBox(player.transform.position,
                player.transform.localScale / 2, Quaternion.identity, k_layerMask);

            foreach (Collider encounterCollider in hitColliders)
            {
                // Non-overlapping neighbor collider
                if (encounterCollider.gameObject != m_lastEnteredCollider)
                {
                    return;
                }
            }

            EncounterZoneLeftTelemetryData telemetryData = new(m_id, player.transform.position);
            TelemetryService.TrackEncounterZoneLeave(SessionManager.Instance.SessionID.ToString(),
                telemetryData);
            m_isPlayerInZone = false;
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