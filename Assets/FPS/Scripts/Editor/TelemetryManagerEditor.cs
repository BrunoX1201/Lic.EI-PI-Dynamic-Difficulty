using FPS.Scripts.Telemetry.Shared;
using Telemetry.Shared;
using Unity.FPS.Game;
using Unity.FPS.Telemetry;
using UnityEditor;
using UnityEngine;

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
        private Item m_item;


        private bool m_showPlayerDeathEvent;
        private bool m_showPlayerAttackEvent;
        private bool m_showItemPickUpEvent;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Separator();
            m_isDebugGroupVisible = EditorGUILayout.BeginFoldoutHeaderGroup(m_isDebugGroupVisible, "Debug");
            if (m_isDebugGroupVisible)
            {
                /* Example to use for any event
                EditorGUILayout.BeginVertical("box");
                m_showPlayerDeathEvent = EditorGUILayout.Foldout(m_showPlayerDeathEvent, "event");
                if (m_showPlayerDeathEvent)
                {
                    if (GUILayout.Button.Button("Publish", GUILayout.Width(80))) TelemetryService.Track...(...);
                }
                EditorGUILayout.EndVertical();
                */

                TelemetryEventBus.IsDebugOn = EditorGUILayout.Toggle("Debug Mode", TelemetryEventBus.IsDebugOn);
                EditorGUILayout.LabelField("Events:");

                HandlePlayerDeathEvent();
                HandlePlayerAttackEvent();
                HandleItemPickedUpEvent();
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
                m_sessionId = EditorGUILayout.IntField("Session Id:", m_sessionId);

                EditorGUILayout.Space(2);
                m_playerId = EditorGUILayout.IntField("Player Id:", m_playerId);

                EditorGUILayout.Space(2);
                m_targetId = EditorGUILayout.IntField("Target Id:", m_targetId);

                EditorGUILayout.Space(2);
                m_isHit = EditorGUILayout.Toggle("Is Hit:", m_isHit);

                EditorGUILayout.Space(2);
                m_accuracy = EditorGUILayout.FloatField("Accuracy:", m_accuracy);

                EditorGUILayout.Space(2);
                m_playerPosition = (Transform)EditorGUILayout.ObjectField("Player Position:", m_playerPosition,
                    typeof(Transform),
                    true);

                EditorGUILayout.Space(2);
                m_weaponUsed = (Weapon)EditorGUILayout.EnumPopup("Weapon Used:", m_weaponUsed);

                EditorGUILayout.Space(2);
                m_damagePerHit = EditorGUILayout.FloatField("Damage Per Hit:", m_damagePerHit);

                EditorGUILayout.Space(2);
                m_attackType = (AttackType)EditorGUILayout.EnumPopup("Attack Type:", m_attackType);

                EditorGUILayout.Space(2);
                m_remainingAmmo = EditorGUILayout.FloatField("Remaining Ammo:", m_remainingAmmo);

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
            m_showPlayerDeathEvent = EditorGUILayout.Foldout(m_showPlayerDeathEvent, "PlayerDied");
            if (m_showPlayerDeathEvent)
            {
                EditorGUILayout.Space(2);
                m_sessionId = EditorGUILayout.IntField("Session Id:", m_sessionId);

                EditorGUILayout.Space(2);
                m_instigator =
                    (Instigator)EditorGUILayout.ObjectField("Instigator:", m_instigator, typeof(Instigator), true);

                EditorGUILayout.Space(2);
                m_deathCount = EditorGUILayout.IntField("Death Count", m_deathCount);

                EditorGUILayout.Space(2);
                m_timeAliveSeconds = EditorGUILayout.IntField("Time Alive (Seconds):", m_timeAliveSeconds);

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

        private void HandleItemPickedUpEvent()
        {
            EditorGUILayout.BeginVertical("box");
            m_showItemPickUpEvent = EditorGUILayout.Foldout(m_showItemPickUpEvent, "ItemPickedUp");
            if (m_showItemPickUpEvent)
            {
                EditorGUILayout.Space(2);
                m_sessionId = EditorGUILayout.IntField("Session Id:", m_sessionId);

                EditorGUILayout.Space(2);
                m_playerId = EditorGUILayout.IntField("Player Id:", m_playerId);

                EditorGUILayout.Space(2);
                m_item = (Item)EditorGUILayout.EnumPopup("Item:", m_item);

                EditorGUILayout.Space(2);
                m_remainingAmmo = EditorGUILayout.FloatField("Remaining Ammo:", m_remainingAmmo);

                EditorGUILayout.Space(2);
                m_remainingHealth = EditorGUILayout.Slider("Remaining Health:", m_remainingHealth, 0f, 100f);

                EditorGUILayout.Space(2);
                m_playerPosition =
                    (Transform)EditorGUILayout.ObjectField("Player Position", m_playerPosition, typeof(Transform),
                        true);


                if (GUILayout.Button("Publish", GUILayout.Width(80)))
                {
                    ItemPickedUpTelemetryData data = new(m_playerId, m_item, m_remainingAmmo, m_remainingHealth,
                        m_playerPosition.localPosition);

                    TelemetryService.TrackItemPickUp(m_sessionId, data);
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}