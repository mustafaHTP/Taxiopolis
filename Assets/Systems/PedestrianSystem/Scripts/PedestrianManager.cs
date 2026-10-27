using Game.Utils;
using UnityEngine;

namespace Game.Systems.PedestrianSystem
{
    /// <summary>
    /// Entry point of the pedestrian system. Initializes the parts and
    /// lets the rest of the game enable or disable the simulation.
    /// </summary>
    public class PedestrianManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private PedestrianSystemSettings[] _pedestrianSystemSettingsArray;

        [Header("Dependencies")]
        [SerializeField] private PedestrianSimSphere _pedestrianSimSphere;

        private bool _isInitialized = false;

        private readonly PedestrianSpawner _pedestrianSpawner = new();
        private readonly PedestrianPointRegistry _pedestrianPointRegistry = new();

        public void Init()
        {
            if (_isInitialized)
            {
                LogX.LogWarning("PedestrianManager has already been initialized.", this);

                return;
            }

            if (_pedestrianPointRegistry == null || _pedestrianSpawner == null || _pedestrianSimSphere == null)
            {
                LogX.LogError("PedestrianManager dependencies are not assigned.", this);

                return;
            }

            PedestrianSystemSettings settings = GetPedestrianSystemSettingsBasedOnPlatform();
            if (settings == null)
            {
                LogX.LogError("Pedestrian system settings not found for current platform!", this);

                return;
            }

            // Order matters: the sim sphere reads points from the registry in its Init.
            _pedestrianPointRegistry.Init(settings);
            _pedestrianSpawner.Init(settings, transform);
            _pedestrianSimSphere.Init(
                settings, 
                _pedestrianPointRegistry, 
                _pedestrianSpawner);

            _isInitialized = true;
        }

        /// <summary>Lets the sim sphere spawn pedestrians again.</summary>
        public void EnableSim()
        {
            if (!_isInitialized) return;

            _pedestrianSimSphere.EnablePedestrianSimulation();
        }

        /// <summary>Stops spawning. Existing pedestrians are still despawned when they leave the life zone.</summary>
        public void DisableSim()
        {
            if (!_isInitialized) return;

            _pedestrianSimSphere.DisablePedestrianSimulation();
        }

        private PedestrianSystemSettings GetPedestrianSystemSettingsBasedOnPlatform()
        {
            if (_pedestrianSystemSettingsArray == null || _pedestrianSystemSettingsArray.Length == 0)
            {
                LogX.LogError("Pedestrian system settings is not set!", this);

                return null;
            }

            CustomPlatform currentPlatform = PlatformUtils.GetPlatform();
            if (currentPlatform == CustomPlatform.Undefined)
            {
                // Use the first settings as a fallback.
                if (_pedestrianSystemSettingsArray[0] == null)
                {
                    LogX.LogError("First element in Pedestrian system settings is null", this);

                    return null;
                }

                return _pedestrianSystemSettingsArray[0];
            }

            for (int i = 0; i < _pedestrianSystemSettingsArray.Length; i++)
            {
                if (_pedestrianSystemSettingsArray[i] == null) continue;

                if (_pedestrianSystemSettingsArray[i].CustomPlatform == currentPlatform)
                {
                    return _pedestrianSystemSettingsArray[i];
                }
            }

            LogX.LogError("The current platform settings was not found in the pedestrian manager", this);

            return null;
        }
    }
}