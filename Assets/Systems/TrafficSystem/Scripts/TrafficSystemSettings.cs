using Game.Systems.TrafficSystem.Pooling;
using Game.Utils;
using UnityEngine;

namespace Game.Systems.TrafficSystem
{
    [CreateAssetMenu(fileName = "New Traffic System Settings", menuName = "Game/Systems/Traffic/Settings")]
    public class TrafficSystemSettings : ScriptableObject
    {
        [SerializeField] private CustomPlatform _customPlatform;
        [SerializeField] private int _vehiclePoolSize = 20;
        [SerializeField] private VehiclePoolProfile _vehiclePoolProfile;
        [Tooltip("Layers that block a spawn (other cars, props...).")]
        [SerializeField] private LayerMask _trafficCheckLayer;
        [Tooltip("Half size of the area that must be free to spawn. Roughly your biggest car body. " +
                 "A fixed box is used, because a car's world bounds change with its rotation.")]
        [SerializeField] private Vector3 _spawnCheckHalfExtents = new(1f, 0.75f, 2.5f);
        [SerializeField] 
        [Tooltip("Should not be bigger than pool size.")]
        private int _activeVehicleLimitInSimSphere = 10;
        [SerializeField] private float _trafficSimInterval = 1f;

        public CustomPlatform CustomPlatform => _customPlatform;
        public int VehiclePoolSize => _vehiclePoolSize;
        public int ActiveVehicleLimitInSimSphere => _activeVehicleLimitInSimSphere;
        public float TrafficSimInterval => _trafficSimInterval;
        public VehiclePoolProfile VehiclePoolProfile => _vehiclePoolProfile;
        public LayerMask TrafficCheckLayer => _trafficCheckLayer;
        public Vector3 SpawnCheckHalfExtents => _spawnCheckHalfExtents;
    }
}
