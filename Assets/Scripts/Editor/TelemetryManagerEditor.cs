using Telemetry;
using Telemetry.Enums;
using Telemetry.Events;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(TelemetryManager))]
    public class TelemetryManagerEditor : UnityEditor.Editor
    {
        private bool m_IsDebugGroupVisible = true;
        private int m_SessionId = 1;
        private int m_PlayerId = 1;
        private int m_TargetId = 2;
        private bool m_IsHit = true;
        private float m_Accuracy = 0.75f;
        private Vector3 m_PlayerPosition = Vector3.zero;
        private Weapon m_WeaponUsed = Weapon.None;
        private float m_DamagePerHit = 10f;
        private AttackType m_AttackType = AttackType.Ranged;
        private float m_RemainingAmmo = 30f;

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

                EditorGUILayout.LabelField("Events:");
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("PlayerAttacked", EditorStyles.boldLabel);
                m_SessionId = EditorGUILayout.IntField("Session Id", m_SessionId);
                m_PlayerId = EditorGUILayout.IntField("Player Id", m_PlayerId);
                m_TargetId = EditorGUILayout.IntField("Target Id", m_TargetId);
                m_IsHit = EditorGUILayout.Toggle("Is Hit", m_IsHit);
                m_Accuracy = EditorGUILayout.FloatField("Accuracy", m_Accuracy);
                m_PlayerPosition = EditorGUILayout.Vector3Field("Player Position", m_PlayerPosition);
                m_WeaponUsed = (Weapon)EditorGUILayout.EnumPopup("Weapon", m_WeaponUsed);
                m_DamagePerHit = EditorGUILayout.FloatField("Damage Per Hit", m_DamagePerHit);
                m_AttackType = (AttackType)EditorGUILayout.EnumPopup("Attack Type", m_AttackType);
                m_RemainingAmmo = EditorGUILayout.FloatField("Remaining Ammo", m_RemainingAmmo);

                if (GUILayout.Button("Track", GUILayout.Width(80)))
                {
                    PlayerAttackedTelemetry telemetryEvent = new(
                        m_SessionId,
                        m_PlayerId,
                        m_TargetId,
                        m_IsHit,
                        m_Accuracy,
                        m_PlayerPosition,
                        m_WeaponUsed,
                        m_DamagePerHit,
                        m_AttackType,
                        m_RemainingAmmo);

                    TelemetryEventBus.Publish(telemetryEvent);
                    Debug.Log("PlayerAttacked telemetry event published.");
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}