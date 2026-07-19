using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.FPS.Game
{
    public class APIBootstrapper : Singleton<APIBootstrapper>
    {
        private const string m_API_SERVICE_APP_DIR = "DDA_AI";
        private const string m_API_SERVICE_APP_NAME = "app.exe";
        private Process m_apiServiceProcess;

        public override void Awake()
        {
            base.Awake();

            string projectPath = Application.dataPath.Substring(0,
                Application.dataPath.LastIndexOf("/", StringComparison.Ordinal));

            string apiServicePath = Path.Combine(projectPath, m_API_SERVICE_APP_DIR);
            m_apiServiceProcess = new Process();
            Debug.Log($"Starting API SERVICE Process: {m_API_SERVICE_APP_NAME}, path: {apiServicePath}");

            m_apiServiceProcess.StartInfo.FileName = Path.Combine(apiServicePath, m_API_SERVICE_APP_NAME);
            m_apiServiceProcess.StartInfo.WorkingDirectory = apiServicePath;
            m_apiServiceProcess.Start();
        }

        private void OnApplicationQuit()
        {
            if (m_apiServiceProcess == null)
            {
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/PID {m_apiServiceProcess.Id} /T /F",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            else
            {
                // REWORK FOR OTHER PLATFORMS
                m_apiServiceProcess.Kill();
            }

            m_apiServiceProcess = null;
        }
    }
}