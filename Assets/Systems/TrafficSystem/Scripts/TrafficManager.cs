using Game.Utils;
using UnityEngine;
using RoadSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Systems.TrafficSystem
{
    /// <summary>
    /// Entry point of the traffic system. Initializes the parts and
    /// lets the rest of the game enable or disable the simulation.
    /// </summary>
    public class TrafficManager : MonoBehaviour
    {
        [SerializeField] private bool _autoInitOnStart = true;

        [Header("Settings")]
        [SerializeField] private TrafficSystemSettings[] _trafficSystemSettingsArray;

        [Header("References")]
        [SerializeField] private TrafficSimSphere _trafficSimSphere;
        [SerializeField] private RoadNetwork _roadNetwork;

        private readonly TrafficSpawner _trafficSpawner = new();
        private bool _hasInitialized = false;

        public void Init()
        {
            if (_hasInitialized)
            {
                LogX.LogWarning("TrafficManager is already initialized.", this);

                return;
            }

            if (_trafficSimSphere == null)
            {
                LogX.LogError($"Traffic Sim Sphere is not assigned in {nameof(TrafficManager)}", this);

                return;
            }

            TrafficSystemSettings trafficSystemSettings = GetTrafficSystemSettingsBasedOnPlatform();
            if (trafficSystemSettings == null)
            {
                LogX.LogError("Traffic system settings not found for current platform!", this);

                return;
            }

            _trafficSpawner.Init(trafficSystemSettings, transform);
            _trafficSimSphere.Init(trafficSystemSettings, _roadNetwork, _trafficSpawner);

            // This flag was never set before, so every EnableSim/DisableSim call re-ran Init.
            _hasInitialized = true;
        }

        public void EnableSim()
        {
            if (!EnsureInitialized()) return;

            _trafficSimSphere.EnableTrafficSimulation();
        }

        /// <summary>Stops spawning. Vehicles that are far away are still despawned.</summary>
        public void DisableSim()
        {
            if (!EnsureInitialized()) return;

            _trafficSimSphere.DisableTrafficSimulation();
        }

        private void Start()
        {
            if (_autoInitOnStart) Init();
        }

        private bool EnsureInitialized()
        {
            if (_hasInitialized) return true;

            LogX.LogWarning("TrafficManager is not initialized. Initializing now...", this);

            Init();

            return _hasInitialized;
        }

        private TrafficSystemSettings GetTrafficSystemSettingsBasedOnPlatform()
        {
            if (_trafficSystemSettingsArray == null || _trafficSystemSettingsArray.Length == 0)
            {
                LogX.LogError("Traffic system settings is not set!", this);

                return null;
            }

            CustomPlatform currentCustomPlatform = PlatformUtils.GetPlatform();
            if (currentCustomPlatform == CustomPlatform.Undefined)
            {
                // use first settings as fallback
                if (_trafficSystemSettingsArray[0] == null)
                {
                    LogX.LogError("First element in traffic system settings is null", this);

                    return null;
                }

                return _trafficSystemSettingsArray[0];
            }

            // find the current platform settings
            for (int i = 0; i < _trafficSystemSettingsArray.Length; i++)
            {
                if (_trafficSystemSettingsArray[i] == null) continue;

                if (_trafficSystemSettingsArray[i].CustomPlatform == currentCustomPlatform)
                {
                    return _trafficSystemSettingsArray[i];
                }
            }

            // if the current platform settings is not found in the array
            LogX.LogError("The current platform settings was not found in the traffic manager", this);

            return null;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TrafficManager))]
    public class TrafficManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            TrafficManager trafficManager = (TrafficManager)target;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Initialize"))
            {
                trafficManager.Init();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Enable Traffic"))
            {
                trafficManager.EnableSim();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Disable Traffic"))
            {
                trafficManager.DisableSim();
            }
        }
    }
#endif
}