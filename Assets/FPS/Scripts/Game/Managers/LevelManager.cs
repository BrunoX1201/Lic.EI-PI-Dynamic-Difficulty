using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Unity.FPS.Game
{
    public enum SceneName
    {
        Unknown = -1,

        // Not level scenes from 0 to 100
        IntroMenu = 0,
        LoseScene = 1,
        WinScene = 2,

        // Level scenes from 200 to 300
        MainScene = 200,
        SecondaryScene = 201,

        // Special cases starting from 400
        CurrentLevel = 400
    }

    public class LevelManager : Singleton<LevelManager>
    {
        public SceneName WinScene;
        public SceneName LoseScene;
        public SceneName FirstLevel;
        private List<SceneName> m_levels;
        private SceneName m_currentLevel;

        public override void Awake()
        {
            base.Awake();
            if (m_isBeingDestroyed) return;

            m_levels = new List<SceneName>
            {
                SceneName.MainScene,
                SceneName.SecondaryScene
            };

            string activeSceneName = SceneManager.GetActiveScene().name;
            if (!Enum.TryParse(activeSceneName, out SceneName activeScene)) return;

            if (m_levels.IndexOf(activeScene) >= 0)
                m_currentLevel = activeScene;
            else
                m_currentLevel = SceneName.Unknown;
        }

        public void LoadScene(SceneName scene)
        {
            int sceneValue = (int)scene;
            if (sceneValue >= 0 && sceneValue <= 100)
            {
                SceneManager.LoadScene(scene.ToString());
                return;
            }

            if (sceneValue >= 200 && sceneValue <= 300)
            {
                m_currentLevel = scene;
                SceneManager.LoadScene(scene.ToString());
                return;
            }

            if (scene == SceneName.CurrentLevel) LoadCurrentLevel();
        }

        public SceneName GetNextLevel()
        {
            int index = m_levels.IndexOf(m_currentLevel);
            if (index < 0) return FirstLevel;

            return index + 1 >= m_levels.Count ? SceneName.Unknown : m_levels[index + 1];
        }

        private void LoadCurrentLevel()
        {
            if (m_currentLevel == SceneName.Unknown)
            {
                LoadFirstLevel();
                return;
            }

            LoadScene(m_currentLevel);
        }

        private void LoadFirstLevel()
        {
            if (m_levels.Count <= 0) return;
            m_currentLevel = FirstLevel;
            LoadScene(FirstLevel);
        }
    }
}