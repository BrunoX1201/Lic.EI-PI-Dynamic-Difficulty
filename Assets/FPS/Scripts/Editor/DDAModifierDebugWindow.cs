using Unity.FPS.DDA;
using Unity.FPS.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Unity.FPS.EditorExt
{
    public class DDAModifierDebugWindow : EditorWindow
    {
        private int m_simTotalMobiles = 1;
        private int m_simTotalBosses = 0;
        private float m_simMobileHealth = 10f;
        private float m_simMobileHitbox = 0.05f;
        private float m_simTurretHealth = 0f;
        private float m_simTurretHitbox = 0f;
        private string m_simAgent = "K_means";
        private string m_simAction = "None";

        [MenuItem("Tools/DDA/Modifier Debug Window")]
        private static void ShowWindow()
        {
            GetWindow<DDAModifierDebugWindow>("DDA Debug");
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Entra em Play Mode para alterar os modificadores em runtime.",
                    MessageType.Info);

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);

            EditorGUILayout.LabelField("Estado atual", EditorStyles.boldLabel);
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("HasReceivedOutput", DDAModifierState.HasReceivedOutput.ToString());
                EditorGUILayout.LabelField("Mobiles",
                    $"{DDAModifierState.TotalMobilesModifier.Value} ({DDAModifierState.TotalMobilesModifier.Direction})");
                EditorGUILayout.LabelField("Bosses",
                    $"{DDAModifierState.TotalTurretsModifier.Value} ({DDAModifierState.TotalTurretsModifier.Direction})");
                EditorGUILayout.LabelField("Mobile Health/Hitbox",
                    $"{DDAModifierState.MobileHealthModifier.Value} / {DDAModifierState.MobileHitboxModifier.Value}");
                EditorGUILayout.LabelField("Turret Health/Hitbox",
                    $"{DDAModifierState.TurretHealthModifier.Value} / {DDAModifierState.TurretHitboxModifier.Value}");
            }
            else
            {
                EditorGUILayout.LabelField("—");
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Simulate Full Prediction (DDAModelOutputReceived)", EditorStyles.boldLabel);

            m_simAgent = EditorGUILayout.TextField("Agent (log only)", m_simAgent);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Mobile", EditorStyles.miniBoldLabel);
            m_simTotalMobiles = EditorGUILayout.IntField("Count", m_simTotalMobiles);
            m_simMobileHealth = EditorGUILayout.FloatField("Health", m_simMobileHealth);
            m_simMobileHitbox = EditorGUILayout.FloatField("Hitbox", m_simMobileHitbox);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Turret", EditorStyles.miniBoldLabel);
            m_simTotalBosses = EditorGUILayout.IntField("Count", m_simTotalBosses);
            m_simTurretHealth = EditorGUILayout.FloatField("Health", m_simTurretHealth);
            m_simTurretHitbox = EditorGUILayout.FloatField("Hitbox", m_simTurretHitbox);

            EditorGUILayout.Space(2);
            m_simAction = EditorGUILayout.TextField("Action (log only)", m_simAction);

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Simulate + Force Spawn"))
            {
                DDAEventManager.Broadcast(new DDAModelOutputReceived(
                    m_simTotalMobiles, m_simTotalBosses,
                    m_simMobileHealth, m_simMobileHitbox,
                    m_simTurretHealth, m_simTurretHitbox,
                    m_simAgent, m_simAction));

                if (EncounterZoneManager.Instance != null)
                    EncounterZoneManager.Instance.ForceSpawnEncounter(DDAModifierState.TotalMobilesModifier.Value, DDAModifierState.TotalTurretsModifier.Value);
                else
                    Debug.LogWarning(
                        "[DDAModifierDebugWindow] EncounterZoneManager.Instance é null — está em Play Mode?");
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}