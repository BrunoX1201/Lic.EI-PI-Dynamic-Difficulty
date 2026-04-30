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
        private readonly TelemetryEventUIState m_encounterStartEventState = new(typeof(EncounterStartedTelemetry));
        private readonly TelemetryEventUIState m_encounterEndEventState = new(typeof(EncounterEndedTelemetry));
        private readonly TelemetryEventUIState m_encounterZoneLeaveState = new(typeof(EncounterZoneLeftTelemetry));
        private readonly TelemetryEventUIState m_encounterZoneEnterState = new(typeof(EncounterZoneEnteredTelemetry));

        private readonly HashSet<TelemetryEventUIState> m_allEventStates = new();
        private readonly HashSet<TelemetryEventUIState> m_suppressedEventStates = new();

        private bool m_isDebugGroupVisible = true;
        private string m_sessionId;

        // Player
        private int m_playerId = 1;

        private Transform m_playerStartPosition;
        private Transform m_playerEndPosition;
        private AirborneType m_playerAirborneType;
        private Transform m_playerCurrentPosition;

        private MapLocationSO m_playerLastLocation;
        private MapLocationSO m_playerNewLocation;
        private bool m_hasPlayerDiscoveredNewLocation;
        private int m_playerTimeInLastLocationSeconds;
        private MapLocationSO m_playerCurrentLocation;

        private float m_playerMoveSpeed = 5f;

        private float m_playerStartHealth = 100f;
        private float m_playerCurrentHealth = 100f;

        private AttackType m_playerAttackType = AttackType.Ranged;
        private Weapon m_playerWeaponUsed;
        private bool m_isHit = true;
        private float m_playerWeaponDamagePerHit = 10f;
        private float m_playerDamageTakenPerHit = 10f;
        private float m_playerCurrentWeaponAmmo = 30f;

        private float m_playerStartTotalAmmo = 100f;
        private float m_playerCurrentTotalAmmo = 100f;

        // Statistic
        private float m_playerAccuracy = 0.75f;
        private int m_playerDeathCount;
        private int m_playerTimeAliveSeconds;
        private int m_totalGameTimeSeconds;

        // Target / Instigator
        private int m_targetId = 2;
        private Instigator m_instigator;

        // Pick Up
        private Item m_itemPickedUp;

        // Encounter
        private string m_encounterId;
        private MapLocationSO m_encounterLocation;
        private EncounterStartReason m_encounterStartReason;
        private EncounterEndReason m_encounterEndReason;
        private int m_encounterTotalEnemies = 10;
        private int m_encounterCurrentEnemies;


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
                HandleEncounterStartEvent();
                HandleEncounterEndEvent();
                HandleEncounterZoneLeaveEvent();
                HandleEncounterZoneEnterEvent();

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
            m_allEventStates.Add(m_encounterStartEventState);
            m_allEventStates.Add(m_encounterEndEventState);
            m_allEventStates.Add(m_encounterZoneEnterState);
            m_allEventStates.Add(m_encounterZoneLeaveState);
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
                m_playerAccuracy = EditorGUILayout.FloatField("Accuracy", m_playerAccuracy);

                EditorGUILayout.Space(2);
                m_playerCurrentPosition = (Transform)EditorGUILayout.ObjectField("Player Position",
                    m_playerCurrentPosition,
                    typeof(Transform),
                    true);

                EditorGUILayout.Space(2);
                m_playerWeaponUsed = (Weapon)EditorGUILayout.EnumPopup("Weapon Used", m_playerWeaponUsed);

                EditorGUILayout.Space(2);
                m_playerWeaponDamagePerHit = EditorGUILayout.FloatField("Damage Per Hit", m_playerWeaponDamagePerHit);

                EditorGUILayout.Space(2);
                m_playerAttackType = (AttackType)EditorGUILayout.EnumPopup("Attack Type", m_playerAttackType);

                EditorGUILayout.Space(2);
                m_playerCurrentWeaponAmmo = EditorGUILayout.FloatField("Remaining Ammo", m_playerCurrentWeaponAmmo);

                EditorGUILayout.Space(2);
                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    PlayerAttackedTelemetryData data = new(m_playerId, m_targetId, m_isHit, m_playerAccuracy,
                        m_playerCurrentPosition.position, m_playerWeaponUsed, m_playerWeaponDamagePerHit,
                        m_playerAttackType,
                        m_playerCurrentWeaponAmmo);

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
                m_playerDeathCount = EditorGUILayout.IntField("Death Count", m_playerDeathCount);

                EditorGUILayout.Space(2);
                m_playerTimeAliveSeconds = EditorGUILayout.IntField("Time Alive (Seconds)", m_playerTimeAliveSeconds);

                EditorGUILayout.Space(2);
                m_playerCurrentPosition =
                    (Transform)EditorGUILayout.ObjectField("Player Position", m_playerCurrentPosition,
                        typeof(Transform),
                        true);

                EditorGUILayout.Space(2);
                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    PlayerDiedTelemetryData data = new(m_instigator, m_playerDeathCount, m_playerTimeAliveSeconds,
                        m_playerCurrentPosition.position);

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
                m_itemPickedUp = (Item)EditorGUILayout.EnumPopup("Item", m_itemPickedUp);

                EditorGUILayout.Space(2);
                m_playerCurrentWeaponAmmo = EditorGUILayout.FloatField("Remaining Ammo", m_playerCurrentWeaponAmmo);

                EditorGUILayout.Space(2);
                m_playerCurrentHealth = EditorGUILayout.Slider("Remaining Health", m_playerCurrentHealth, 0f, 100f);

                EditorGUILayout.Space(2);
                m_playerCurrentPosition =
                    (Transform)EditorGUILayout.ObjectField("Player Position", m_playerCurrentPosition,
                        typeof(Transform),
                        true);


                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    ItemPickedUpTelemetryData data = new(m_playerId, m_itemPickedUp, m_playerCurrentWeaponAmmo,
                        m_playerCurrentHealth,
                        m_playerCurrentPosition.position);

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
                m_playerDamageTakenPerHit =
                    EditorGUILayout.FloatField("Damage Taken Per Hit", m_playerDamageTakenPerHit);

                EditorGUILayout.Space(2);
                m_playerMoveSpeed = EditorGUILayout.FloatField("Player Move Speed", m_playerMoveSpeed);

                EditorGUILayout.Space(2);
                m_instigator =
                    (Instigator)EditorGUILayout.ObjectField("Instigator", m_instigator, typeof(Instigator), true);

                EditorGUILayout.Space(2);
                m_playerCurrentHealth =
                    EditorGUILayout.Slider("Player Current Health", m_playerCurrentHealth, 0f, 100f);

                EditorGUILayout.Space(2);
                m_playerCurrentPosition =
                    (Transform)EditorGUILayout.ObjectField("Player Position", m_playerCurrentPosition,
                        typeof(Transform),
                        true);


                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    PlayerTookDamageTelemetryData data = new(m_playerDamageTakenPerHit, m_playerMoveSpeed, m_instigator,
                        m_playerCurrentHealth, m_playerCurrentPosition.position
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
                m_playerLastLocation = (MapLocationSO)EditorGUILayout.ObjectField("Last Location", m_playerLastLocation,
                    typeof(MapLocationSO), false);

                EditorGUILayout.Space(2);
                m_playerNewLocation = (MapLocationSO)EditorGUILayout.ObjectField("New Location", m_playerNewLocation,
                    typeof(MapLocationSO), false);

                EditorGUILayout.Space(2);
                m_playerTimeInLastLocationSeconds =
                    EditorGUILayout.IntField("Time In Last Location (Seconds)", m_playerTimeInLastLocationSeconds);

                EditorGUILayout.Space(2);
                m_hasPlayerDiscoveredNewLocation =
                    EditorGUILayout.Toggle("Has Discovered", m_hasPlayerDiscoveredNewLocation);

                EditorGUILayout.Space(2);
                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    NewLocationDiscoveredTelemetryData data = new(m_playerLastLocation, m_playerNewLocation,
                        m_playerTimeInLastLocationSeconds,
                        m_hasPlayerDiscoveredNewLocation);

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
                m_playerWeaponUsed = (Weapon)EditorGUILayout.EnumPopup("Weapon Used", m_playerWeaponUsed);

                EditorGUILayout.Space(2);
                m_playerCurrentWeaponAmmo = EditorGUILayout.FloatField("Remaining Ammo", m_playerCurrentWeaponAmmo);

                EditorGUILayout.Space(2);
                m_playerCurrentHealth = EditorGUILayout.Slider("Remaining Health", m_playerCurrentHealth, 0f, 100f);

                EditorGUILayout.Space(2);
                m_playerCurrentPosition =
                    (Transform)EditorGUILayout.ObjectField("Player Position", m_playerCurrentPosition,
                        typeof(Transform),
                        true);

                EditorGUILayout.Space(2);
                m_playerCurrentLocation =
                    (MapLocationSO)EditorGUILayout.ObjectField("Player Location", m_playerCurrentLocation,
                        typeof(MapLocationSO),
                        true);

                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    TargetKilledTelemetryData data = new(m_targetId, m_playerWeaponUsed, m_playerCurrentWeaponAmmo,
                        m_playerCurrentHealth,
                        m_playerCurrentPosition.position, m_playerCurrentLocation);

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
                m_playerCurrentLocation =
                    (MapLocationSO)EditorGUILayout.ObjectField("Player Location", m_playerCurrentLocation,
                        typeof(MapLocationSO),
                        true);

                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    GameTimePassedTelemetryData data = new(m_totalGameTimeSeconds, m_playerCurrentLocation);

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
                m_playerStartPosition = (Transform)EditorGUILayout.ObjectField("Start Position", m_playerStartPosition,
                    typeof(Transform), true);

                EditorGUILayout.Space(2);
                m_playerEndPosition =
                    (Transform)EditorGUILayout.ObjectField("End Position", m_playerEndPosition, typeof(Transform),
                        true);

                EditorGUILayout.Space(2);
                m_playerAirborneType = (AirborneType)EditorGUILayout.EnumPopup("Airborne Type", m_playerAirborneType);

                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    PlayerWentAirborneTelemetryData data = new(m_playerStartPosition.position,
                        m_playerEndPosition.position,
                        m_playerAirborneType);

                    TelemetryService.TrackPlayerGoAirborne(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void HandleEncounterStartEvent()
        {
            EditorGUILayout.BeginVertical("box");

            DrawEventFoldout("EncounterStarted", m_encounterStartEventState);
            if (m_encounterStartEventState.IsExpanded)
            {
                EditorGUILayout.Space(2);
                m_encounterId = EditorGUILayout.TextField("Encounter Id", m_encounterId);

                EditorGUILayout.Space(2);
                m_encounterStartReason =
                    (EncounterStartReason)EditorGUILayout.EnumPopup("Start Reason", m_encounterStartReason);

                EditorGUILayout.Space(2);
                m_encounterTotalEnemies = EditorGUILayout.IntField("Total Enemies", m_encounterTotalEnemies);

                EditorGUILayout.Space(2);
                m_playerStartHealth =
                    EditorGUILayout.Slider("Player Start Health", m_playerStartHealth, 0f, 100f);

                EditorGUILayout.Space(2);
                m_playerStartTotalAmmo = EditorGUILayout.FloatField("Player Start Ammo", m_playerStartTotalAmmo);

                EditorGUILayout.Space(2);
                m_encounterLocation =
                    (MapLocationSO)EditorGUILayout.ObjectField("Encounter Location", m_encounterLocation,
                        typeof(MapLocationSO), true);

                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    EncounterStartedTelemetryData data = new(m_encounterId, m_encounterStartReason,
                        m_encounterTotalEnemies,
                        m_playerStartHealth, m_playerStartTotalAmmo, m_encounterLocation);

                    TelemetryService.TrackEncounterStart(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void HandleEncounterEndEvent()
        {
            EditorGUILayout.BeginVertical("box");

            DrawEventFoldout("EncounterEnded", m_encounterEndEventState);
            if (m_encounterEndEventState.IsExpanded)
            {
                EditorGUILayout.Space(2);
                m_encounterId = EditorGUILayout.TextField("Encounter Id", m_encounterId);

                EditorGUILayout.Space(2);
                m_encounterEndReason =
                    (EncounterEndReason)EditorGUILayout.EnumPopup("End Reason", m_encounterEndReason);

                EditorGUILayout.Space(2);
                m_encounterCurrentEnemies = EditorGUILayout.IntField("Remaining Enemies", m_encounterCurrentEnemies);

                EditorGUILayout.Space(2);
                m_playerCurrentHealth =
                    EditorGUILayout.Slider("Player Remaining Health", m_playerCurrentHealth, 0f, 100f);

                EditorGUILayout.Space(2);
                m_playerCurrentTotalAmmo =
                    EditorGUILayout.FloatField("Player Remaining Ammo", m_playerCurrentTotalAmmo);

                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    EncounterEndedTelemetryData data = new(m_encounterId, m_encounterEndReason, m_playerCurrentHealth,
                        m_playerCurrentTotalAmmo, m_encounterCurrentEnemies);

                    TelemetryService.TrackEncounterEnd(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void HandleEncounterZoneEnterEvent()
        {
            EditorGUILayout.BeginVertical("box");

            DrawEventFoldout("EncounterZoneEntered", m_encounterZoneEnterState);
            if (m_encounterZoneEnterState.IsExpanded)
            {
                EditorGUILayout.Space(2);
                m_encounterId = EditorGUILayout.TextField("Encounter Id", m_encounterId);

                EditorGUILayout.Space(2);
                m_playerCurrentPosition =
                    (Transform)EditorGUILayout.ObjectField("Player Position", m_playerCurrentPosition,
                        typeof(Transform),
                        true);

                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    EncounterZoneEnteredTelemetryData data = new(m_encounterId, m_playerCurrentPosition.position);

                    TelemetryService.TrackEncounterZoneEnter(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void HandleEncounterZoneLeaveEvent()
        {
            EditorGUILayout.BeginVertical("box");

            DrawEventFoldout("EncounterZoneLeft", m_encounterZoneLeaveState);
            if (m_encounterZoneLeaveState.IsExpanded)
            {
                EditorGUILayout.Space(2);
                m_encounterId = EditorGUILayout.TextField("Encounter Id", m_encounterId);

                EditorGUILayout.Space(2);
                m_playerCurrentPosition =
                    (Transform)EditorGUILayout.ObjectField("Player Position", m_playerCurrentPosition,
                        typeof(Transform),
                        true);

                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    EncounterZoneLeftTelemetryData data = new(m_encounterId, m_playerCurrentPosition.position);

                    TelemetryService.TrackEncounterZoneLeave(m_sessionId, data);
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