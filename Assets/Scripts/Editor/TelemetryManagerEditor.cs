using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(TelemetryManager))]
    public class TelemetryManagerEditor : UnityEditor.Editor
    {
        private bool m_IsDebugGroupVisible = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();


            EditorGUILayout.Separator();
            m_IsDebugGroupVisible = EditorGUILayout.BeginFoldoutHeaderGroup(m_IsDebugGroupVisible, "Debug");
            if (m_IsDebugGroupVisible)
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