using Unity.FPS.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Unity.FPS.EditorExt
{
    [CustomEditor(typeof(DoorController))]
    public class DoorControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DoorController doorController = (DoorController)target;

            EditorGUILayout.Space(4);

            string btnLabel = doorController.IsOpened ? "Close" : "Open";
            if (GUILayout.Button(btnLabel))
            {
                if (doorController.IsOpened)
                {
                    doorController.Close();
                }
                else
                {
                    doorController.Open();
                }
            }
        }
    }
}