using Telemetry;
using Telemetry.Events;
using Telemetry.Shared;
using Unity.FPS.Game;
using UnityEditor;
using UnityEngine;

namespace Unity.FPS.EditorExt
{
    [CustomEditor(typeof(TelemetryManager))]
    public class TelemetryManagerEditor : Editor
    {
        private bool m_IsDebugGroupVisible = true;
        private int m_sessionId = 1;
        private int m_playerId = 1;
        private int m_targetId = 2;
        private bool m_isHit = true;
        private float m_accuracy = 0.75f;
        private Vector3 m_playerPosition = Vector3.zero;
        private Weapon m_weaponUsed;
        private float m_damagePerHit = 10f;
        private AttackType m_attackType = AttackType.Ranged;
        private float m_remainingAmmo = 30f;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Separator();
            m_IsDebugGroupVisible = EditorGUILayout.BeginFoldoutHeaderGroup(m_IsDebugGroupVisible, "Debug");
            if (m_IsDebugGroupVisible)
            {
                /* Example to use for any event
                EditorGUILayout.BeginHorizontal("box");
                GUILayout.Label("Example");
                if (GUILayout.Button("Track", GUILayout.Width(80))) TelemetryService.Track...(...);
                EditorGUILayout.EndHorizontal();
                */

                TelemetryEventBus.IsDebugOn = EditorGUILayout.Toggle("Debug Mode", TelemetryEventBus.IsDebugOn);
                EditorGUILayout.LabelField("Events:");

                HandlePlayerAttackEvent();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

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
    }
}