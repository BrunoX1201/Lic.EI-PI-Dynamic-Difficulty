using Managers;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(TelemetryManager))]
    public class TelemetryManagerEditor : UnityEditor.Editor
    {
        private bool m_isDebugGroupVisible = true;

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
                GUILayout.Label("Example");
                if (GUILayout.Button("Track", GUILayout.Width(80))) TelemetryService.Track...(...);
                EditorGUILayout.EndHorizontal();
                */
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}