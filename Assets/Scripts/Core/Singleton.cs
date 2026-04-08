using UnityEngine;

namespace Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = (T)FindAnyObjectByType(typeof(T));
                    if (s_instance == null)
                    {
                        SetupInstance();
                    }
                }

                return s_instance;
            }
        }

        private static T s_instance;


        public virtual void Awake()
        {
            RemoveDuplicates();
        }

        private static void SetupInstance()
        {
            s_instance = (T)FindAnyObjectByType(typeof(T));

            if (s_instance == null)
            {
                GameObject gameObj = new()
                {
                    name = typeof(T).Name
                };
                s_instance = gameObj.AddComponent<T>();
                DontDestroyOnLoad(gameObj);
            }
        }

        private void RemoveDuplicates()
        {
            if (s_instance == null)
            {
                s_instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}