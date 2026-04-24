using System;
using System.Collections.Generic;
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
        private class TelemetryEventUIState
        {
            public readonly Type EventType;
            public bool IsExpanded;
            public bool IsSuppressed;

            public TelemetryEventUIState(Type eventType)
            {
                EventType = eventType;
            }
        }

        private readonly TelemetryEventUIState m_playerDeathEventState = new(typeof(PlayerDiedTelemetry));
        private readonly TelemetryEventUIState m_playerAttackEventState = new(typeof(PlayerAttackedTelemetry));
        private readonly TelemetryEventUIState m_itemPickUpEventState = new(typeof(ItemPickedUpTelemetry));

        private readonly TelemetryEventUIState m_newLocationDiscoverEventState =
            new(typeof(NewLocationDiscoveredTelemetry));

        private readonly TelemetryEventUIState m_playerTakeDamageEventState = new(typeof(PlayerTookDamageTelemetry));
        private readonly TelemetryEventUIState m_targetKillEventState = new(typeof(TargetKilledTelemetry));
        private readonly TelemetryEventUIState m_gameTimePassEventState = new(typeof(GameTimePassedTelemetry));
        private readonly TelemetryEventUIState m_playerGoAirborneEventState = new(typeof(PlayerWentAirborneTelemetry));

        private readonly HashSet<TelemetryEventUIState> m_allEventStates = new();
        private readonly HashSet<TelemetryEventUIState> m_suppressedEventStates = new();

        private float m_remainingHealth = 100f;
        private bool m_isDebugGroupVisible = true;
        private string m_sessionId;
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
        private Transform m_startPosition;
        private Transform m_endPosition;
        private AirborneType m_airborneType;

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

                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Common Settings", EditorStyles.boldLabel);

                HandleSessionId();

                EditorGUILayout.EndVertical();


                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
                if (GUILayout.Button(m_suppressedEventStates.Count == 0 ? "Select all" : "Clear all",
                        GUILayout.Width(80)))
                {
                    if (m_suppressedEventStates.Count == 0)
                    {
                        foreach (TelemetryEventUIState evt in m_allEventStates)
                        {
                            EnableEventSuppression(evt);
                        }
                    }
                    else
                    {
                        HashSet<TelemetryEventUIState> setCopy = new(m_suppressedEventStates);
                        foreach (TelemetryEventUIState evt in setCopy)
                        {
                            DisableEventSuppression(evt);
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();

                HandlePlayerDeathEvent();
                HandlePlayerAttackEvent();
                HandleItemPickUpEvent();
                HandleNewLocationDiscoverEvent();
                HandleTargetKillEvent();
                HandlePlayerTakeDamageEvent();
                HandleGameTimePassEvent();
                HandlePlayerGoAirborneEvent();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void OnEnable()
        {
            m_sessionId = SessionManager.Instance.SessionID.ToString();

            m_allEventStates.Add(m_playerDeathEventState);
            m_allEventStates.Add(m_playerAttackEventState);
            m_allEventStates.Add(m_itemPickUpEventState);
            m_allEventStates.Add(m_newLocationDiscoverEventState);
            m_allEventStates.Add(m_playerTakeDamageEventState);
            m_allEventStates.Add(m_targetKillEventState);
            m_allEventStates.Add(m_gameTimePassEventState);
            m_allEventStates.Add(m_playerGoAirborneEventState);
        }

        private void HandleSessionId()
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Session Id", GUILayout.Width(80));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(m_sessionId);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Generate", GUILayout.Width(80)))
            {
                m_sessionId = Guid.NewGuid().ToString();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void HandleTelemetrySettingsFolderPath()
        {
            TelemetryManager manager = (TelemetryManager)target;
            bool uploaderPathExists = Directory.Exists(manager.TelemetryServiceSettings.UploaderBaseFilePath);

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Telemetry Storage Path");

            EditorGUI.BeginDisabledGroup(!uploaderPathExists);
            bool openPressed = GUILayout.Button("Open", GUILayout.Width(60));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.SelectableLabel(
                manager.TelemetryServiceSettings.UploaderBaseFilePath,
                EditorStyles.textField,
                GUILayout.Height(EditorGUIUtility.singleLineHeight)
            );

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

        private void HandlePlayerAttackEvent()
        {
            EditorGUILayout.BeginVertical("box");

            DrawEventFoldout("PlayerAttacked", m_playerAttackEventState);
            if (m_playerAttackEventState.IsExpanded)
            {
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

        private void HandlePlayerDeathEvent()
        {
            EditorGUILayout.BeginVertical("box");

            DrawEventFoldout("PlayerDied", m_playerDeathEventState);
            if (m_playerDeathEventState.IsExpanded)
            {
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

            DrawEventFoldout("ItemPickedUp", m_itemPickUpEventState);
            if (m_itemPickUpEventState.IsExpanded)
            {
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

            DrawEventFoldout("PlayerTookDamage", m_playerTakeDamageEventState);
            if (m_playerTakeDamageEventState.IsExpanded)
            {
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

            DrawEventFoldout("NewLocationDiscovered", m_newLocationDiscoverEventState);
            if (m_newLocationDiscoverEventState.IsExpanded)
            {
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

            DrawEventFoldout("TargetKilled", m_targetKillEventState);
            if (m_targetKillEventState.IsExpanded)
            {
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

            DrawEventFoldout("GameTimePassed", m_gameTimePassEventState);
            if (m_gameTimePassEventState.IsExpanded)
            {
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

        private void HandlePlayerGoAirborneEvent()
        {
            EditorGUILayout.BeginVertical("box");

            DrawEventFoldout("PlayerWentAirborne", m_playerGoAirborneEventState);
            if (m_playerGoAirborneEventState.IsExpanded)
            {
                EditorGUILayout.Space(2);
                m_startPosition = (Transform)EditorGUILayout.ObjectField("Start Position", m_startPosition,
                    typeof(Transform), true);

                EditorGUILayout.Space(2);
                m_endPosition =
                    (Transform)EditorGUILayout.ObjectField("End Position", m_endPosition, typeof(Transform), true);

                EditorGUILayout.Space(2);
                m_airborneType = (AirborneType)EditorGUILayout.EnumPopup("Airborne Type", m_airborneType);

                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    PlayerWentAirborneTelemetryData data = new(m_startPosition.position, m_endPosition.position,
                        m_airborneType);

                    TelemetryService.TrackPlayerGoAirborne(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEventFoldout(string evtName, TelemetryEventUIState evtUIState)
        {
            Rect headerRect = EditorGUILayout.GetControlRect();

            float checkboxWidth = 20f;
            float checkboxLabelWidth = 65f;

            Rect buttonRect = new(headerRect.xMax - checkboxWidth - checkboxLabelWidth, headerRect.y,
                checkboxWidth + checkboxLabelWidth, headerRect.height);
            Rect checkboxLabelRect = new(buttonRect.x, buttonRect.y, checkboxLabelWidth,
                buttonRect.height);
            Rect checkboxRect = new(buttonRect.x + checkboxLabelWidth, buttonRect.y, checkboxWidth, buttonRect.height);

            if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
            {
                ToggleEventSuppression(evtUIState);
            }

            EditorGUI.LabelField(checkboxLabelRect, "Suppressed", EditorStyles.miniLabel);
            EditorGUI.Toggle(checkboxRect, evtUIState.IsSuppressed);

            Rect foldoutRect = new(headerRect.x, headerRect.y, headerRect.width - checkboxWidth - checkboxLabelWidth,
                headerRect.height);
            evtUIState.IsExpanded = EditorGUI.Foldout(foldoutRect, evtUIState.IsExpanded, evtName, true);
        }

        private void ToggleEventSuppression(TelemetryEventUIState evtUIState)
        {
            if (evtUIState.IsSuppressed)
            {
                DisableEventSuppression(evtUIState);
            }
            else
            {
                EnableEventSuppression(evtUIState);
            }
        }

        private void EnableEventSuppression(TelemetryEventUIState evtUIState)
        {
            evtUIState.IsSuppressed = true;
            m_suppressedEventStates.Add(evtUIState);
            TelemetryEventBus.SuppressEvent(evtUIState.EventType);
        }

        private void DisableEventSuppression(TelemetryEventUIState evtUIState)
        {
            evtUIState.IsSuppressed = false;
            m_suppressedEventStates.Remove(evtUIState);
            TelemetryEventBus.UnsuppressEvent(evtUIState.EventType);
        }
    }
}