using Unity.FPS.DDA;
using Unity.FPS.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Unity.FPS.EditorExt
{
    public class DDAModifierDebugWindow : EditorWindow
    {
        private int m_simTotalEnemies = 5;
        private float m_simEnemyHealth = 1f;
        private float m_simEnemyHitbox = 1f;
        private Agent m_simAgent;
        private DDADirection m_simAction;

        [MenuItem("Tools/DDA/Modifier Debug Window")]
        private static void ShowWindow()
        {
            GetWindow<DDAModifierDebugWindow>("DDA Debug");
        }
        
        private void OnInspectorUpdate()
        {
            // Se estiver em Play Mode, força o OnGUI a rodar a cada frame do editor por causa do "Estado atual"
            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Entra em Play Mode para alterar os modificadores em runtime.",
                    MessageType.Info);

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);

            EditorGUILayout.LabelField("Estado atual", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("HasReceivedOutput", Application.isPlaying
                ? DDAModifierState.HasReceivedOutput.ToString()
                : "-");
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("TotalEnemies",
                    $"{DDAModifierState.TotalEnemiesModifier.Value} ({DDAModifierState.TotalEnemiesModifier.Direction})");
                EditorGUILayout.LabelField("EnemyHealth",
                    $"{DDAModifierState.EnemyHealthModifier.Value} ({DDAModifierState.EnemyHealthModifier.Direction})");
                EditorGUILayout.LabelField("EnemyHitbox",
                    $"{DDAModifierState.EnemyHitboxModifier.Value} ({DDAModifierState.EnemyHitboxModifier.Direction})");
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Simulate Full Prediction (DDAModelOutputReceived)", EditorStyles.boldLabel);
            m_simTotalEnemies = EditorGUILayout.IntField("Total Enemies", m_simTotalEnemies);
            m_simEnemyHealth = EditorGUILayout.FloatField("Enemy Health", m_simEnemyHealth);
            m_simEnemyHitbox = EditorGUILayout.FloatField("Enemy Hitbox", m_simEnemyHitbox);
            m_simAgent = EditorGUILayout.EnumPopup("Agent", m_simAgent) as Agent? ?? m_simAgent;
            m_simAction = EditorGUILayout.EnumPopup("Action", m_simAction) as DDADirection? ?? m_simAction;

            if (GUILayout.Button("Simulate"))
            {
                DDAEventManager.Broadcast(new DDAModelOutputReceived(
                    m_simTotalEnemies, m_simEnemyHealth, m_simEnemyHitbox, m_simAgent.ToString(),
                    m_simAction.ToString()));

                if (EncounterZoneManager.Instance != null)
                    EncounterZoneManager.Instance.ForceSpawnEncounter(m_simTotalEnemies);
                else
                    Debug.LogWarning(
                        "[DDAModifierDebugWindow] EncounterZoneManager.Instance é null — está em Play Mode?");
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}