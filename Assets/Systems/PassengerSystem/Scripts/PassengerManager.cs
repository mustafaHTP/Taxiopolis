using Game.Systems.PassengerSystem.Passengers;
using Game.Utils;
using UnityEngine;

namespace Game.Systems.PassengerSystem
{
    /// <summary>
    /// Entry point of the passenger system. Initializes the parts and
    /// lets the rest of the game enable or disable the simulation.
    /// </summary>
    public class PassengerManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _autoInitOnStart;
        [SerializeField] private GameObject _passengerWaitingPointPrefab;
        [SerializeField] private GameObject _passengerDestinationPointPrefab;
        [SerializeField] private PassengerSystemSettings[] _passengerSystemSettingsArray;

        [Header("Dependencies")]
        [SerializeField] private PassengerSimSphere _passengerSimSphere;

        private bool _isInitialized = false;
        private readonly PassengerPointRegistry _passengerPointRegistry = new();
        private readonly PassengerSpawner _passengerSpawner = new();

        public GameObject PassengerWaitingPointPrefab => _passengerWaitingPointPrefab;
        public GameObject PassengerDestinationPointPrefab => _passengerDestinationPointPrefab;
        public PassengerJobHelper PassengerJobHelper { get; private set; }

        public void Init()
        {
            if (_isInitialized)
            {
                LogX.LogWarning("PassengerManager has already been initialized.", this);

                return;
            }

            if (_passengerPointRegistry == null
                || _passengerSpawner == null
                || _passengerSimSphere == null
                || _passengerSystemSettingsArray == null)
            {
                LogX.LogError("PassengerManager dependencies are not assigned.", this);

                return;
            }

            PassengerSystemSettings passengerSystemSettings = GetPassengerSystemSettingsBasedOnPlatform();
            if (passengerSystemSettings == null)
            {
                LogX.LogError("Passenger system settings not found for current platform!", this);

                return;
            }

            PassengerJobHelper = new PassengerJobHelper(passengerSystemSettings);

            // Order matters: the spawner needs the job helper, and the sim sphere reads the registry.
            _passengerPointRegistry.Init(passengerSystemSettings);
            _passengerSpawner.Init(
                passengerSystemSettings, 
                PassengerJobHelper,
                _passengerPointRegistry,
                transform);
            _passengerSimSphere.Init(
                passengerSystemSettings, 
                _passengerPointRegistry, 
                _passengerSpawner);

            _isInitialized = true;
        }

        public void EnableSim()
        {
            if (!_isInitialized) return;

            _passengerSimSphere.EnablePassengerSimulation();
        }

        /// <summary>Stops spawning. Passengers that are far away are still despawned.</summary>
        public void DisableSim()
        {
            if (!_isInitialized) return;

            _passengerSimSphere.DisablePassengerSimulation();
        }

        /// <summary>
        /// Called by the car when it accepted a passenger. The passenger leaves the simulation.
        /// </summary>
        public void NotifyPassengerPickedUp(Passenger passenger)
        {
            if (!_isInitialized) return;

            _passengerSpawner.ReleaseFromSimulation(passenger);
        }

        /// <summary>
        /// Called by the car when the passenger left it. The passenger is managed by the simulation again.
        /// </summary>
        public void NotifyPassengerDroppedOff(Passenger passenger)
        {
            if (!_isInitialized) return;

            _passengerSpawner.ReturnToSimulation(passenger);
        }

        private PassengerSystemSettings GetPassengerSystemSettingsBasedOnPlatform()
        {
            if (_passengerSystemSettingsArray == null || _passengerSystemSettingsArray.Length == 0)
            {
                LogX.LogError("Passenger system settings is not set!", this);

                return null;
            }

            CustomPlatform currentCustomPlatform = PlatformUtils.GetPlatform();
            if (currentCustomPlatform == CustomPlatform.Undefined)
            {
                // Use the first settings as a fallback.
                if (_passengerSystemSettingsArray[0] == null)
                {
                    LogX.LogError("First element in passenger system settings is null", this);

                    return null;
                }

                return _passengerSystemSettingsArray[0];
            }

            for (int i = 0; i < _passengerSystemSettingsArray.Length; i++)
            {
                if (_passengerSystemSettingsArray[i] == null) continue;

                if (_passengerSystemSettingsArray[i].CustomPlatform == currentCustomPlatform)
                {
                    return _passengerSystemSettingsArray[i];
                }
            }

            LogX.LogError("The current platform settings was not found in the passenger manager", this);

            return null;
        }

        private void Start()
        {
            if (_autoInitOnStart)
            {
                Init();
            }
        }
    }
}