using System.Threading.Tasks;
using Unity.FPS.Test;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.FPS.EditorExt
{
    public class TestServiceWindow : EditorWindow
    {
        private static TestService s_service;
        private bool m_isLoading;
        private string m_testOutput;

        private string
            m_testingSessionID = "651b5f50-a9aa-42e0-974a-f299a8bc1a33"; // session id of shared data test in DDA system

        [MenuItem("Tools/TestService")]
        private static void ShowWindow()
        {
            GetWindow<TestServiceWindow>("Test Service");
        }

        private void OnEnable()
        {
            if (s_service != null)
            {
                return;
            }

            s_service = new TestService();
        }

        private void OnGUI()
        {
            bool isTestingScene = SceneManager.GetActiveScene().name == "TestScene";

            if (!isTestingScene)
            {
                EditorGUILayout.HelpBox("This window only works in TestScene", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Initialized: {s_service.Initialized}", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(m_isLoading);


            m_testingSessionID = EditorGUILayout.TextField("Testing Session ID", m_testingSessionID);
            EditorGUI.BeginDisabledGroup(s_service.Initialized);
            bool initializePressed = GUILayout.Button("Initialize Service");
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!s_service.Initialized);
            bool restartPressed = GUILayout.Button("Restart Service");
            EditorGUI.EndDisabledGroup();

            if (initializePressed)
            {
                _ = InitializeService();
            }

            if (restartPressed)
            {
                _ = RestartService();
            }

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Available Tests", EditorStyles.boldLabel);

            EditorGUILayout.Space(2);

            EditorGUI.BeginDisabledGroup(!s_service.Initialized);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Test DDA Process Encounter");
            if (GUILayout.Button("Run", GUILayout.Width(80)))
            {
                RunTest(s_service.TestDDAProcessEncounter());
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

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
            await s_service.Initialize(m_testingSessionID);
            m_isLoading = false;
        }

        private async Task RestartService()
        {
            m_isLoading = true;
            await s_service.Restart(m_testingSessionID);
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