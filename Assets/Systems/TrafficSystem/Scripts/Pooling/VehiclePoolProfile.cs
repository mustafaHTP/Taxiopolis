using UnityEngine;

namespace Game.Systems.TrafficSystem.Pooling
{
    [CreateAssetMenu(
        fileName = "VehiclePoolProfile",
        menuName = "RoadSystem/Pooling/Vehicle Pool Profile")]
    public class VehiclePoolProfile : ScriptableObject
    {
        [SerializeField] private VehicleSpawnEntry[] _vehicleSpawnEntries;

        public VehicleSpawnEntry[] VehicleSpawnEntries => _vehicleSpawnEntries;
    }
}
