using Unity.FPS.Game;
using UnityEditor;

namespace Unity.FPS.EditorExt
{
    [CustomEditor(typeof(SessionManager))]
    public class SessionManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SessionManager sessionManager = (SessionManager)target;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Session Id");

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(sessionManager.SessionID.ToString());
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }
    }
}