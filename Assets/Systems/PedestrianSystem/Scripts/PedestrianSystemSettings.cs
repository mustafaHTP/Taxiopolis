using Game.Systems.PedestrianSystem.Pooling;
using Game.Utils;
using UnityEngine;

namespace Game.Systems.PedestrianSystem
{
    [CreateAssetMenu(fileName = "New Pedestrian System Settings", menuName = "Game/Systems/Pedestrian/Settings")]
    public class PedestrianSystemSettings : ScriptableObject
    {
        [SerializeField] private CustomPlatform _customPlatform;
        [SerializeField] private PedestrianPoolProfile _pedestrianPoolProfile;
        [SerializeField] private float _minPedestrianSpeed = 0.5f;
        [SerializeField] private float _maxPedestrianSpeed = 1.3f;
        [SerializeField] private int _pedestrianPoolSize = 20;
        [SerializeField]
        [Tooltip("Used for spatial hash grid to optimize passenger point queries. Half to full of the max spawn distance is a good start.")]
        private float _gridCellSize = 15f;
        [SerializeField]
        [Tooltip("Should not be bigger than pool size.")]
        private int _activePedestrianLimitInSimSphere = 10;
        [SerializeField] private float _pedestrianSimInterval = 1f;

        public CustomPlatform CustomPlatform => _customPlatform;
        public int PedestrianPoolSize => _pedestrianPoolSize;
        public int ActivePedestrianLimitInSimSphere => _activePedestrianLimitInSimSphere;
        public float PedestrianSimInterval => _pedestrianSimInterval;
        public float MinPedestrianSpeed => _minPedestrianSpeed;
        public float MaxPedestrianSpeed => _maxPedestrianSpeed;
        public PedestrianPoolProfile PedestrianPoolProfile => _pedestrianPoolProfile;
        public float GridCellSize => _gridCellSize;

        private void OnValidate()
        {
            _pedestrianPoolSize = Mathf.Max(1, _pedestrianPoolSize);
            _activePedestrianLimitInSimSphere = Mathf.Clamp(_activePedestrianLimitInSimSphere, 0, _pedestrianPoolSize);
            _pedestrianSimInterval = Mathf.Max(0.05f, _pedestrianSimInterval);
            _minPedestrianSpeed = Mathf.Max(0f, _minPedestrianSpeed);
            _maxPedestrianSpeed = Mathf.Max(_minPedestrianSpeed, _maxPedestrianSpeed);
        }
    }
}
