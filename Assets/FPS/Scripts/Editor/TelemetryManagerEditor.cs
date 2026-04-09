using Unity.FPS.Game;
using Unity.FPS.Telemetry;
using UnityEditor;
using UnityEngine;

namespace Unity.FPS.EditorExt
{
    [CustomEditor(typeof(TelemetryManager))]
    public class TelemetryManagerEditor : Editor
    {
        private bool m_isDebugGroupVisible = true;
        private int m_sessionId;
        private int m_deathCount;
        private int m_timeAliveSeconds;
        private Transform m_playerPosition;
        private Instigator m_instigator;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();


            EditorGUILayout.Separator();
            m_isDebugGroupVisible = EditorGUILayout.BeginFoldoutHeaderGroup(m_isDebugGroupVisible, "Debug");
            if (m_isDebugGroupVisible)
            {
                EditorGUILayout.LabelField("Events:");

                /* Example to use for any event
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.Label("Example");
                if (EditorGUILayout.Button("Track", EditorGUILayout.Width(80))) TelemetryService.Track...(...);
                EditorGUILayout.EndHorizontal();
                */

                HandlePlayerDeathEvent();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void HandlePlayerDeathEvent()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Player Died");

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
                (Transform)EditorGUILayout.ObjectField("Player Position", m_playerPosition, typeof(Transform), true);

            EditorGUILayout.Space(2);
            if (GUILayout.Button("Publish", GUILayout.Width(80)))
            {
                PlayerDiedTelemetryData data = new(m_instigator, m_deathCount, m_timeAliveSeconds,
                    m_playerPosition.position);

                TelemetryService.TrackPlayerDeath(m_sessionId, data);
            }


            EditorGUILayout.EndVertical();
        }
    }
}