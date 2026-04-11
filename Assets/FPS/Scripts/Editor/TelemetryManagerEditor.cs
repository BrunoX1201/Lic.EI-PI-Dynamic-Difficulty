using Telemetry;
using Telemetry.Events;
using Telemetry.Shared;
using Unity.FPS.Game;
using UnityEditor;
using Unity.FPS.Telemetry;
using UnityEngine;

namespace Unity.FPS.EditorExt
{
    [CustomEditor(typeof(TelemetryManager))]
    public class TelemetryManagerEditor : Editor
    {
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

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Separator();
            m_isDebugGroupVisible = EditorGUILayout.BeginFoldoutHeaderGroup(m_isDebugGroupVisible, "Debug");
            if (m_isDebugGroupVisible)
            {
                /* Example to use for any event
                EditorGUILayout.BeginHorizontal("box");
                m_showPlayerDeathEvent = EditorGUILayout.Foldout(m_showPlayerDeathEvent, "event");
                if (m_showPlayerDeathEvent)
                {
                    if (EditorGUILayout.Button("Publish", EditorGUILayout.Width(80))) TelemetryService.Track...(...);
                }
                EditorGUILayout.EndHorizontal();
                */

                HandlePlayerDeathEvent();

                TelemetryEventBus.IsDebugOn = EditorGUILayout.Toggle("Debug Mode", TelemetryEventBus.IsDebugOn);
                EditorGUILayout.LabelField("Events:");

                HandlePlayerAttackEvent();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        private int m_deathCount;

        private void HandlePlayerAttackEvent()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("PlayerAttacked", EditorStyles.boldLabel);
            m_sessionId = EditorGUILayout.IntField("Session Id", m_sessionId);
            m_playerId = EditorGUILayout.IntField("Player Id", m_playerId);
            m_targetId = EditorGUILayout.IntField("Target Id", m_targetId);
            m_isHit = EditorGUILayout.Toggle("Is Hit", m_isHit);
            m_accuracy = EditorGUILayout.FloatField("Accuracy", m_accuracy);
            m_playerPosition = EditorGUILayout.Vector3Field("Player Position", m_playerPosition);
            m_weaponUsed = (Weapon)EditorGUILayout.EnumPopup("Weapon Used", m_weaponUsed);
            m_damagePerHit = EditorGUILayout.FloatField("Damage Per Hit", m_damagePerHit);
            m_attackType = (AttackType)EditorGUILayout.EnumPopup("Attack Type", m_attackType);
            m_remainingAmmo = EditorGUILayout.FloatField("Remaining Ammo", m_remainingAmmo);

            if (GUILayout.Button("Publish", GUILayout.Width(80)))
            {
                TelemetryService.TrackPlayerAttack(
                    m_sessionId,
                    new PlayerAttackedTelemetryData(
                        m_playerId,
                        m_targetId,
                        m_isHit,
                        m_accuracy,
                        m_playerPosition,
                        m_weaponUsed,
                        m_damagePerHit,
                        m_attackType,
                        m_remainingAmmo));
                Debug.Log("PlayerAttacked telemetry event published.");
            }
        }
        private int m_timeAliveSeconds;
        private Instigator m_instigator;

        private bool m_showPlayerDeathEvent;

        private void HandlePlayerDeathEvent()
        {
            EditorGUILayout.BeginVertical("box");
            m_showPlayerDeathEvent = EditorGUILayout.Foldout(m_showPlayerDeathEvent, "Player Died");
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
    }
}