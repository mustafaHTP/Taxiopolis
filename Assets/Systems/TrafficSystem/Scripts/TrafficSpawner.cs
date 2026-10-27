using Game.AI;
using Game.Car;
using Game.Systems.TrafficSystem.Pooling;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems.TrafficSystem
{
    /// <summary>
    /// Spawns traffic vehicles at the road spawn points it is given (if the spot is free)
    /// and despawns the vehicles it is given. It does not decide where or how many.
    /// That is the sim sphere's job.
    /// </summary>
    public class TrafficSpawner
    {
        private bool _hasInitialized = false;
        private readonly List<GameObject> _activeVehiclesInTraffic = new();
        private VehiclePoolProfile _vehiclePoolProfile;
        private Vector3 _spawnCheckHalfExtents;
        private LayerMask _trafficCheckLayer;
        private VehiclePool _vehiclePool;
        private Transform _spawnRoot;

        public IReadOnlyList<GameObject> ActiveVehiclesInTraffic => _activeVehiclesInTraffic;

        public void Init(
            TrafficSystemSettings trafficSystemSettings, 
            Transform spawnRoot)
        {
            if (_hasInitialized)
            {
                LogX.LogWarning($"{nameof(TrafficSpawner)} is already initialized.");

                return;
            }

            _vehiclePoolProfile = trafficSystemSettings.VehiclePoolProfile;
            _spawnCheckHalfExtents = trafficSystemSettings.SpawnCheckHalfExtents;
            _trafficCheckLayer = trafficSystemSettings.TrafficCheckLayer;
            _spawnRoot = spawnRoot;

            _vehiclePool = new VehiclePool(
                _vehiclePoolProfile,
                trafficSystemSettings.VehiclePoolSize,
                _spawnRoot);
            _vehiclePool.Init();

            _hasInitialized = true;
        }

        /// <summary>
        /// Tries to spawn vehicles at the given road spawn points, in list order, until
        /// <paramref name="spawnCount"/> vehicles were actually spawned.
        /// Blocked points do not use up the budget. Shuffle the points beforehand for random placement.
        /// </summary>
        public void SpawnTraffic(IReadOnlyList<RoadSpawnPoint> roadSpawnPoints, int spawnCount)
        {
            if (!_hasInitialized) return;
            if (roadSpawnPoints == null || roadSpawnPoints.Count == 0) return;
            if (spawnCount <= 0) return;

            // Bring the physics world up to date with everything that moved since the last
            // physics step, so the blocked check below sees the real positions.
            Physics.SyncTransforms();

            int spawned = 0;
            for (int i = 0; i < roadSpawnPoints.Count && spawned < spawnCount; i++)
            {
                SpawnResult result = TrySpawnAt(roadSpawnPoints[i]);

                if (result == SpawnResult.Spawned)
                {
                    spawned++;
                }
                else if (result == SpawnResult.PoolExhausted)
                {
                    // No free vehicle, so every remaining point would fail the same way.
                    LogX.LogWarning("Vehicle pool is exhausted.");

                    break;
                }
            }
        }

        /// <summary>Tries to spawn one vehicle at every given road spawn point.</summary>
        public void SpawnTraffic(IReadOnlyList<RoadSpawnPoint> roadSpawnPoints)
        {
            SpawnTraffic(roadSpawnPoints, int.MaxValue);
        }

        public void DespawnVehicles(IReadOnlyList<GameObject> vehiclesToDespawn)
        {
            if (!_hasInitialized) return;
            if (vehiclesToDespawn == null || vehiclesToDespawn.Count == 0) return;

            for (int i = 0; i < vehiclesToDespawn.Count; i++)
            {
                GameObject vehicle = vehiclesToDespawn[i];

                // Remove returns false if it was not tracked, so no separate Contains is needed.
                if (_activeVehiclesInTraffic.Remove(vehicle))
                {
                    _vehiclePool.Return(vehicle);
                }
                else
                {
                    LogX.LogWarning("Attempted to despawn a vehicle that is not managed by the traffic spawner.");
                }
            }
        }

        private enum SpawnResult
        {
            Spawned,
            Skipped,
            PoolExhausted
        }

        private SpawnResult TrySpawnAt(RoadSpawnPoint spawnPoint)
        {
            if (spawnPoint.RoadPoint == null)
            {
                LogX.LogError($"RoadPoint is null for spawn point: {spawnPoint.name}. Skipping spawn.", spawnPoint);

                return SpawnResult.Skipped;
            }

            Transform spawnTransform = spawnPoint.transform;

            // Lift the box by its half height so it sits on the road instead of inside it.
            Vector3 center = spawnTransform.position + Vector3.up * _spawnCheckHalfExtents.y;

            // Check BEFORE taking a vehicle from the pool, so a blocked point does not
            // activate and deactivate a whole car (AI, rigidbody, sounds...) every tick.
            // CheckBox does not allocate, and trigger colliders never block a spawn.
            bool isBlocked = Physics.CheckBox(
                center,
                _spawnCheckHalfExtents,
                spawnTransform.rotation,
                _trafficCheckLayer,
                QueryTriggerInteraction.Ignore);

            DrawX.Debug.DrawBox(
                center,
                _spawnCheckHalfExtents,
                spawnTransform.rotation,
                isBlocked ? Color.red : Color.green,
                0f);

            if (isBlocked) return SpawnResult.Skipped;

            GameObject vehicle = _vehiclePool.Get();
            if (vehicle == null) return SpawnResult.PoolExhausted;

            if (!vehicle.TryGetComponent(out CarAIHandler carAIHandler))
            {
                LogX.LogError($"Car does not have a {nameof(CarAIHandler)} component attached.", vehicle);

                _vehiclePool.Return(vehicle);

                return SpawnResult.Skipped;
            }

            vehicle.transform.SetPositionAndRotation(center, spawnTransform.rotation);

            // Make the physics world aware of this car now, otherwise the next spawn point
            // checked in the same call cannot see it and two cars could spawn overlapping.
            Physics.SyncTransforms();

            carAIHandler.SetRoadPoint(spawnPoint.RoadPoint);

            _activeVehiclesInTraffic.Add(vehicle);

            return SpawnResult.Spawned;
        }
    }
}