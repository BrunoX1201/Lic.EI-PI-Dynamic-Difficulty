using System.Threading.Tasks;
using Unity.FPS.Test;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.FPS.EditorExt
{
    public class TestServiceWindow : EditorWindow
    {
        private TestService m_service;
        private bool m_isLoading;
        private string m_testOutput;

        [MenuItem("Tools/TestService")]
        private static void ShowWindow()
        {
            GetWindow<TestServiceWindow>("Test Service");
        }

        private void OnEnable()
        {
            m_service = new TestService();
        }

        private void OnGUI()
        {
            bool isTestingScene = SceneManager.GetActiveScene().name == "TestScene";

            if (!isTestingScene)
            {
                EditorGUILayout.HelpBox("This window only works in TestScene", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Initialized: {m_service.Initialized}", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(m_isLoading);

            bool initializePressed = GUILayout.Button("Initialize Service");

            if (initializePressed)
            {
                _ = InitializeService();
            }

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Available Tests", EditorStyles.boldLabel);

            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Test DDA Process Encounter");
            if (GUILayout.Button("Run", GUILayout.Width(80)))
            {
                RunTest(m_service.TestDDAProcessEncounter());
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Output");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea(m_testOutput, GUILayout.MinHeight(60));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();


            EditorGUI.EndDisabledGroup();
        }

        private async Task InitializeService()
        {
            m_isLoading = true;
            await m_service.Initialize();
            m_isLoading = false;
        }

        private async Task RunTest<T>(Task<TestResult<T>> test)
        {
            m_isLoading = true;
            TestResult<T> result = await test;

            m_testOutput = $"{result.MetricName} ({(result.Success ? "Success" : "Failure")}):\n{result.MetricValue}";
            m_isLoading = false;
        }
    }
}