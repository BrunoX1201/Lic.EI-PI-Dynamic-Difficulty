using FPS.Scripts.Telemetry.Shared;
using Telemetry.Shared;
using Unity.FPS.Game;
using Unity.FPS.Telemetry;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Unity.FPS.EditorExt
{
    [CustomEditor(typeof(TelemetryManager))]
    public class TelemetryManagerEditor : Editor
    {
        private float m_remainingHealth = 100f;
        private bool m_isDebugGroupVisible = true;
        private int m_sessionId = 1;
        private int m_playerId = 1;
        private int m_targetId = 2;
        private Transform m_playerPosition;
        private bool m_isHit = true;
        private float m_accuracy = 0.75f;
        private Weapon m_weaponUsed;
        private float m_damagePerHit = 10f;
        private AttackType m_attackType = AttackType.Ranged;
        private float m_remainingAmmo = 30f;
        private int m_deathCount;
        private int m_timeAliveSeconds;
        private Instigator m_instigator;
        private MapLocationSO m_lastLocation;
        private MapLocationSO m_newLocation;
        private Item m_item;
        private int m_timeInLastLocationSeconds;
        private bool m_isDiscovered;
        private MapLocationSO m_playerLocation;
        private float m_damageTakenPerHit = 10f;
        private float m_playerMoveSpeed = 5f;
        private float m_playerCurrentHealth = 100f;
        private int m_totalGameTimeSeconds;

        private bool m_showPlayerDeathEvent;
        private bool m_showPlayerAttackEvent;
        private bool m_showItemPickUpEvent;
        private bool m_showNewLocationDiscoverEvent;
        private bool m_showPlayerTakeDamageEvent;
        private bool m_showTargetKillEvent;
        private bool m_showGameTimePassEvent;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            HandleTelemetrySettingsFolderPath();

            EditorGUILayout.Separator();
            m_isDebugGroupVisible = EditorGUILayout.BeginFoldoutHeaderGroup(m_isDebugGroupVisible, "Debug");
            if (m_isDebugGroupVisible)
            {
                /* Example to use for any event
                EditorGUILayout.BeginVertical("box");
                m_showPlayerDeathEvent = EditorGUILayout.Foldout(m_showPlayerDeathEvent, "event");
                if (m_showPlayerDeathEvent)
                {
                    if (GUILayout.Button("Publish", GUILayout.Width(80))) TelemetryService.Track...(...);
                }
                EditorGUILayout.EndVertical();
                */

                TelemetryEventBus.IsDebugOn = EditorGUILayout.Toggle("Debug Mode", TelemetryEventBus.IsDebugOn);
                EditorGUILayout.LabelField("Events:");

                HandlePlayerDeathEvent();
                HandlePlayerAttackEvent();
                HandleItemPickUpEvent();
                HandleNewLocationDiscoverEvent();
                HandleTargetKillEvent();
                HandlePlayerTakeDamageEvent();
                HandleGameTimePassEvent();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void HandlePlayerAttackEvent()
        {
            EditorGUILayout.BeginVertical("box");
            m_showPlayerAttackEvent = EditorGUILayout.Foldout(m_showPlayerAttackEvent, "PlayerAttacked");
            if (m_showPlayerAttackEvent)
            {
                EditorGUILayout.Space(2);
                m_sessionId = EditorGUILayout.IntField("Session Id", m_sessionId);

                EditorGUILayout.Space(2);
                m_playerId = EditorGUILayout.IntField("Player Id", m_playerId);

                EditorGUILayout.Space(2);
                m_targetId = EditorGUILayout.IntField("Target Id", m_targetId);

                EditorGUILayout.Space(2);
                m_isHit = EditorGUILayout.Toggle("Is Hit", m_isHit);

                EditorGUILayout.Space(2);
                m_accuracy = EditorGUILayout.FloatField("Accuracy", m_accuracy);

                EditorGUILayout.Space(2);
                m_playerPosition = (Transform)EditorGUILayout.ObjectField("Player Position", m_playerPosition,
                    typeof(Transform),
                    true);

                EditorGUILayout.Space(2);
                m_weaponUsed = (Weapon)EditorGUILayout.EnumPopup("Weapon Used", m_weaponUsed);

                EditorGUILayout.Space(2);
                m_damagePerHit = EditorGUILayout.FloatField("Damage Per Hit", m_damagePerHit);

                EditorGUILayout.Space(2);
                m_attackType = (AttackType)EditorGUILayout.EnumPopup("Attack Type", m_attackType);

                EditorGUILayout.Space(2);
                m_remainingAmmo = EditorGUILayout.FloatField("Remaining Ammo", m_remainingAmmo);

                EditorGUILayout.Space(2);
                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    PlayerAttackedTelemetryData data = new(m_playerId, m_targetId, m_isHit, m_accuracy,
                        m_playerPosition.position, m_weaponUsed, m_damagePerHit, m_attackType, m_remainingAmmo);

                    TelemetryService.TrackPlayerAttack(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void HandleTelemetrySettingsFolderPath()
        {
            TelemetryManager manager = (TelemetryManager)target;

            EditorGUILayout.LabelField("Telemetry Storage Path");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.SelectableLabel(
                manager.TelemetryServiceSettings.UploaderBaseFilePath,
                EditorStyles.textField,
                GUILayout.Height(EditorGUIUtility.singleLineHeight)
            );

            bool uploaderPathExists = Directory.Exists(manager.TelemetryServiceSettings.UploaderBaseFilePath);

            EditorGUI.BeginDisabledGroup(!uploaderPathExists);
            bool openPressed = GUILayout.Button("Open", GUILayout.Width(60));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            if (!uploaderPathExists)
            {
                EditorGUILayout.HelpBox("Path has not been created, run once to be automatically created!",
                    MessageType.Warning);
            }

            if (openPressed && uploaderPathExists)
            {
                EditorUtility.RevealInFinder(manager.TelemetryServiceSettings.UploaderBaseFilePath);
            }
        }

        private void HandlePlayerDeathEvent()
        {
            EditorGUILayout.BeginVertical("box");
            m_showPlayerDeathEvent = EditorGUILayout.Foldout(m_showPlayerDeathEvent, "PlayerDied");
            if (m_showPlayerDeathEvent)
            {
                EditorGUILayout.Space(2);
                m_sessionId = EditorGUILayout.IntField("Session Id", m_sessionId);

                EditorGUILayout.Space(2);
                m_instigator =
                    (Instigator)EditorGUILayout.ObjectField("Instigator", m_instigator, typeof(Instigator), true);

                EditorGUILayout.Space(2);
                m_deathCount = EditorGUILayout.IntField("Death Count", m_deathCount);

                EditorGUILayout.Space(2);
                m_timeAliveSeconds = EditorGUILayout.IntField("Time Alive (Seconds)", m_timeAliveSeconds);

                EditorGUILayout.Space(2);
                m_playerPosition =
                    (Transform)EditorGUILayout.ObjectField("Player Position", m_playerPosition, typeof(Transform),
                        true);

                EditorGUILayout.Space(2);
                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    PlayerDiedTelemetryData data = new(m_instigator, m_deathCount, m_timeAliveSeconds,
                        m_playerPosition.position);

                    TelemetryService.TrackPlayerDeath(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void HandleItemPickUpEvent()
        {
            EditorGUILayout.BeginVertical("box");
            m_showItemPickUpEvent = EditorGUILayout.Foldout(m_showItemPickUpEvent, "ItemPickedUp");
            if (m_showItemPickUpEvent)
            {
                EditorGUILayout.Space(2);
                m_sessionId = EditorGUILayout.IntField("Session Id", m_sessionId);

                EditorGUILayout.Space(2);
                m_playerId = EditorGUILayout.IntField("Player Id", m_playerId);

                EditorGUILayout.Space(2);
                m_item = (Item)EditorGUILayout.EnumPopup("Item", m_item);

                EditorGUILayout.Space(2);
                m_remainingAmmo = EditorGUILayout.FloatField("Remaining Ammo", m_remainingAmmo);

                EditorGUILayout.Space(2);
                m_remainingHealth = EditorGUILayout.Slider("Remaining Health", m_remainingHealth, 0f, 100f);

                EditorGUILayout.Space(2);
                m_playerPosition =
                    (Transform)EditorGUILayout.ObjectField("Player Position", m_playerPosition, typeof(Transform),
                        true);


                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    ItemPickedUpTelemetryData data = new(m_playerId, m_item, m_remainingAmmo, m_remainingHealth,
                        m_playerPosition.position);

                    TelemetryService.TrackItemPickUp(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void HandlePlayerTakeDamageEvent()
        {
            EditorGUILayout.BeginVertical("box");
            m_showPlayerTakeDamageEvent = EditorGUILayout.Foldout(m_showPlayerTakeDamageEvent, "PlayerTookDamage");
            if (m_showPlayerTakeDamageEvent)
            {
                EditorGUILayout.Space(2);
                m_sessionId = EditorGUILayout.IntField("Session Id", m_sessionId);

                EditorGUILayout.Space(2);
                m_damageTakenPerHit = EditorGUILayout.FloatField("Damage Taken Per Hit", m_damageTakenPerHit);

                EditorGUILayout.Space(2);
                m_playerMoveSpeed = EditorGUILayout.FloatField("Player Move Speed", m_playerMoveSpeed);

                EditorGUILayout.Space(2);
                m_instigator =
                    (Instigator)EditorGUILayout.ObjectField("Instigator", m_instigator, typeof(Instigator), true);

                EditorGUILayout.Space(2);
                m_playerCurrentHealth =
                    EditorGUILayout.Slider("Player Current Health", m_playerCurrentHealth, 0f, 100f);

                EditorGUILayout.Space(2);
                m_playerPosition =
                    (Transform)EditorGUILayout.ObjectField("Player Position", m_playerPosition, typeof(Transform),
                        true);


                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    PlayerTookDamageTelemetryData data = new(m_damageTakenPerHit, m_playerMoveSpeed, m_instigator,
                        m_playerCurrentHealth, m_playerPosition.position
                    );

                    TelemetryService.TrackPlayerTakeDamage(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void HandleNewLocationDiscoverEvent()
        {
            EditorGUILayout.BeginVertical("box");
            m_showNewLocationDiscoverEvent =
                EditorGUILayout.Foldout(m_showNewLocationDiscoverEvent, "NewLocationDiscovered");
            if (m_showNewLocationDiscoverEvent)
            {
                EditorGUILayout.Space(2);
                m_sessionId = EditorGUILayout.IntField("Session Id", m_sessionId);

                EditorGUILayout.Space(2);
                m_lastLocation = (MapLocationSO)EditorGUILayout.ObjectField("Last Location", m_lastLocation,
                    typeof(MapLocationSO), false);

                EditorGUILayout.Space(2);
                m_newLocation = (MapLocationSO)EditorGUILayout.ObjectField("New Location", m_newLocation,
                    typeof(MapLocationSO), false);

                EditorGUILayout.Space(2);
                m_timeInLastLocationSeconds =
                    EditorGUILayout.IntField("Time In Last Location (Seconds)", m_timeInLastLocationSeconds);

                EditorGUILayout.Space(2);
                m_isDiscovered = EditorGUILayout.Toggle("Has Discovered", m_isDiscovered);

                EditorGUILayout.Space(2);
                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    NewLocationDiscoveredTelemetryData data = new(m_lastLocation, m_newLocation,
                        m_timeInLastLocationSeconds,
                        m_isDiscovered);

                    TelemetryService.TrackNewLocationDiscover(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void HandleTargetKillEvent()
        {
            EditorGUILayout.BeginVertical("box");
            m_showTargetKillEvent = EditorGUILayout.Foldout(m_showTargetKillEvent, "TargetKilled");
            if (m_showTargetKillEvent)
            {
                EditorGUILayout.Space(2);
                m_sessionId = EditorGUILayout.IntField("Session Id", m_sessionId);

                EditorGUILayout.Space(2);
                m_targetId = EditorGUILayout.IntField("Target Id", m_targetId);

                EditorGUILayout.Space(2);
                m_weaponUsed = (Weapon)EditorGUILayout.EnumPopup("Weapon Used", m_weaponUsed);

                EditorGUILayout.Space(2);
                m_remainingAmmo = EditorGUILayout.FloatField("Remaining Ammo", m_remainingAmmo);

                EditorGUILayout.Space(2);
                m_remainingHealth = EditorGUILayout.Slider("Remaining Health", m_remainingHealth, 0f, 100f);

                EditorGUILayout.Space(2);
                m_playerPosition =
                    (Transform)EditorGUILayout.ObjectField("Player Position", m_playerPosition, typeof(Transform),
                        true);

                EditorGUILayout.Space(2);
                m_playerLocation =
                    (MapLocationSO)EditorGUILayout.ObjectField("Player Location", m_playerLocation,
                        typeof(MapLocationSO),
                        true);

                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    TargetKilledTelemetryData data = new(m_targetId, m_weaponUsed, m_remainingAmmo, m_remainingHealth,
                        m_playerPosition.position, m_playerLocation);

                    TelemetryService.TrackTargetKill(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void HandleGameTimePassEvent()
        {
            EditorGUILayout.BeginVertical("box");
            m_showGameTimePassEvent = EditorGUILayout.Foldout(m_showGameTimePassEvent, "GameTimePassed");
            if (m_showGameTimePassEvent)
            {
                EditorGUILayout.Space(2);
                m_sessionId = EditorGUILayout.IntField("Session Id", m_sessionId);

                EditorGUILayout.Space(2);
                m_totalGameTimeSeconds = EditorGUILayout.IntField("Total Game Time (Seconds)", m_totalGameTimeSeconds);

                EditorGUILayout.Space(2);
                m_playerLocation =
                    (MapLocationSO)EditorGUILayout.ObjectField("Player Location", m_playerLocation,
                        typeof(MapLocationSO),
                        true);

                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    GameTimePassedTelemetryData data = new(m_totalGameTimeSeconds, m_playerLocation);

                    TelemetryService.TrackGameTimePass(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}