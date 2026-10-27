using System;
using UnityEngine;

namespace Game.Systems.TrafficSystem.Pooling
{
    [Serializable]
    public class VehicleSpawnEntry
    {
        [SerializeField] private GameObject _vehiclePrefab;

        public GameObject VehiclePrefab => _vehiclePrefab;
    }
}
