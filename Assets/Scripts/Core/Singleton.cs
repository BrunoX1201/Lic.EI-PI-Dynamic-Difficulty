using UnityEngine;

namespace Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindAnyObjectByType(typeof(T));
                    if (_instance == null) SetupInstance();
                }

                return _instance;
            }
        }

        private static T _instance;

        public virtual void Awake()
        {
            RemoveDuplicates();
        }

        private static void SetupInstance()
        {
            _instance = (T)FindAnyObjectByType(typeof(T));

            if (_instance == null)
            {
                GameObject gameObj = new()
                {
                    name = typeof(T).Name
                };
                _instance = gameObj.AddComponent<T>();
                DontDestroyOnLoad(gameObj);
            }
        }

        private void RemoveDuplicates()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}