using UnityEngine;

namespace Framework.Singleton
{
    public class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        [Header("Persistent Singleton Settings")]
        [SerializeField] private bool _isUnparentOnAwakeEnabled = true;
        [Space(10)]

        protected static T s_Instance;

        public static T Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindAnyObjectByType<T>();
                    if (s_Instance == null)
                    {
                        Debug.LogError($"{nameof(T)} instance not found in the scene.");
                    }
                }
                return s_Instance;
            }
        }

        protected virtual void Awake()
        {
            InitializeSingleton();
        }

        private void InitializeSingleton()
        {
            if (s_Instance == null)
            {
                s_Instance = this as T;

                if (_isUnparentOnAwakeEnabled)
                {
                    transform.SetParent(null);
                }

                DontDestroyOnLoad(gameObject);
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
