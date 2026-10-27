using Game.Utils;
using RoadSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Systems.TrafficSystem
{
    /// <summary>
    /// Decides WHERE and WHEN traffic should exist around this object
    /// (put it on the player car, or on an empty child of it).
    /// It only checks conditions and tells the spawner what to do. The spawner does the work.
    ///
    /// Runs in FixedUpdate on purpose: the spawner queries physics to find free spawn spots.
    ///
    /// Distances from this object, in order:
    ///   spawnMin <= visibleSpawnMin <= spawnMax < despawn
    ///
    /// A spawn point can spawn a vehicle when it is inside the spawn ring (spawnMin..spawnMax)
    /// and either it is outside the camera view, or it is at least visibleSpawnMin away
    /// (far enough that popping in is not noticeable).
    /// Vehicles beyond despawn are returned to the pool, but only while the camera cannot see them.
    /// </summary>
    public class TrafficSimSphere : MonoBehaviour
    {
        [Header("Gizmos Settings")]
        [SerializeField] private bool _debugDrawSphere = true;
        [SerializeField] private Color _debugSpawnMinColor = Color.red;
        [SerializeField] private Color _debugVisibleSpawnMinColor = Color.cyan;
        [SerializeField] private Color _debugSpawnMaxColor = Color.green;
        [SerializeField] private Color _debugDespawnColor = Color.yellow;


        [Header("General Settings")]
        [SerializeField] private bool _enableSimulationUpdates = false;

        [Header("Distances")]
        [FormerlySerializedAs("_deadzoneDistance")]
        [Tooltip("Nothing spawns closer than this.")]
        [SerializeField] private float _spawnMinDistance = 10f;

        [Tooltip("Points INSIDE the camera view must be at least this far to spawn. " +
                 "Points outside the view can spawn from Spawn Min. " +
                 "Top-down camera: keep close to Spawn Min. Chase camera: raise it.")]
        [SerializeField] private float _visibleSpawnMinDistance = 30f;

        [Tooltip("Nothing spawns farther than this.")]
        [SerializeField] private float _spawnMaxDistance = 40f;

        [FormerlySerializedAs("_lifeZoneDistance")]
        [Tooltip("Vehicles farther than this are despawned (only while out of camera view). " +
                 "Keep it clearly above Spawn Max.")]
        [SerializeField] private float _despawnDistance = 45f;

        // Both values come from TrafficSystemSettings in Init.
        private int _maxActiveVehiclesInSphere;
        private float _trafficSimInterval;

        // Reused every tick so the simulation does not allocate.
        private readonly List<GameObject> _vehiclesToDespawn = new();
        private readonly List<RoadSpawnPoint> _nearbySpawnPoints = new();
        private readonly List<RoadSpawnPoint> _candidateSpawnPoints = new();
        private readonly Plane[] _frustumPlanes = new Plane[6];

        private RoadNetwork _roadNetwork;
        private TrafficSpawner _trafficSpawner;
        private bool _hasInitialized = false;
        private float _nextTrafficSimTime;

        public void Init(TrafficSystemSettings trafficSystemSettings, RoadNetwork roadNetwork, TrafficSpawner trafficSpawner)
        {
            if (_hasInitialized)
            {
                LogX.LogWarning("TrafficSimSphere is already initialized.", this);

                return;
            }

            _roadNetwork = roadNetwork;
            _trafficSpawner = trafficSpawner;

            // The road graph (and its spawn point grid) must be built before this.
            if (_roadNetwork.RoadSpawnPoints == null)
            {
                LogX.LogError("Road network is not constructed yet. Call ConstructRoadGraph before initializing traffic.", this);

                return;
            }

            _trafficSimInterval = trafficSystemSettings.TrafficSimInterval;
            _maxActiveVehiclesInSphere = trafficSystemSettings.ActiveVehicleLimitInSimSphere;

            _hasInitialized = true;
        }

        public void EnableTrafficSimulation()
        {
            if (!_hasInitialized)
            {
                LogX.LogWarning("TrafficSimSphere is not initialized. Call Init first.", this);

                return;
            }

            _enableSimulationUpdates = true;
        }

        public void DisableTrafficSimulation()
        {
            _enableSimulationUpdates = false;
        }

        /// <summary>Lets the camera system adapt to the current camera mode at runtime.</summary>
        public void SetVisibleSpawnMinDistance(float distance)
        {
            _visibleSpawnMinDistance = Mathf.Clamp(distance, _spawnMinDistance, _spawnMaxDistance);
        }

        private void FixedUpdate()
        {
            if (!_hasInitialized) return;
            if (Time.time < _nextTrafficSimTime) return;

            _nextTrafficSimTime = Time.time + _trafficSimInterval;

            Camera cam = Camera.main;
            if (cam == null) return;

            // Computed once per tick, shared by the despawn and spawn checks.
            GeometryUtility.CalculateFrustumPlanes(cam, _frustumPlanes);

            Vector3 center = transform.position;
            float despawnSqr = _despawnDistance * _despawnDistance;

            // One pass over the active vehicles:
            // count the ones inside the despawn distance, collect the ones to remove.
            _vehiclesToDespawn.Clear();
            int insideCount = 0;
            IReadOnlyList<GameObject> active = _trafficSpawner.ActiveVehiclesInTraffic;
            for (int i = 0; i < active.Count; i++)
            {
                Vector3 position = active[i].transform.position;
                float distSqr = (position - center).sqrMagnitude;

                if (distSqr <= despawnSqr)
                {
                    insideCount++;

                    continue;
                }

                // Far away, but never remove a vehicle the player can see.
                if (CameraUtils.IsPointInFrustum(_frustumPlanes, position)) continue;

                _vehiclesToDespawn.Add(active[i]);
            }

            // DESPAWN: always runs, even at full capacity or when the simulation is disabled.
            _trafficSpawner.DespawnVehicles(_vehiclesToDespawn);

            if (!_enableSimulationUpdates) return;

            // SPAWN: only if there is room left.
            int spawnCount = _maxActiveVehiclesInSphere - insideCount;
            if (spawnCount <= 0) return;

            CollectSpawnCandidates(center);
            if (_candidateSpawnPoints.Count == 0) return;

            // Shuffle so the spawner does not always pick the same points first.
            Shuffle(_candidateSpawnPoints);
            _trafficSpawner.SpawnTraffic(_candidateSpawnPoints, spawnCount);
        }

        /// <summary>
        /// Fills _candidateSpawnPoints with every spawn point that may spawn a vehicle right now:
        /// inside the spawn ring, and either outside the camera view or far enough away.
        /// Whether the spot is physically free is checked later by the spawner.
        /// </summary>
        private void CollectSpawnCandidates(Vector3 center)
        {
            _candidateSpawnPoints.Clear();

            float minSqr = _spawnMinDistance * _spawnMinDistance;
            float maxSqr = _spawnMaxDistance * _spawnMaxDistance;
            float visibleMinSqr = _visibleSpawnMinDistance * _visibleSpawnMinDistance;

            // The road network's grid removes points that are obviously too far away.
            // The exact checks below still decide.
            _roadNetwork.QueryNearbySpawnPoints(center, _spawnMaxDistance, _nearbySpawnPoints);

            for (int i = 0; i < _nearbySpawnPoints.Count; i++)
            {
                RoadSpawnPoint point = _nearbySpawnPoints[i];
                Vector3 pointPosition = point.transform.position;
                float distSqr = (pointPosition - center).sqrMagnitude;

                if (distSqr <= minSqr || distSqr > maxSqr) continue;

                // Far enough: accepted whether visible or not, so the camera test is skipped.
                // Only closer points must be outside the camera view.
                if (distSqr < visibleMinSqr
                    && CameraUtils.IsPointInFrustum(_frustumPlanes, pointPosition))
                {
                    continue;
                }

                _candidateSpawnPoints.Add(point);
            }
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // Editor only: keeps the distances in a valid order while typing in the Inspector.
        private void OnValidate()
        {
            _spawnMinDistance = Mathf.Max(0f, _spawnMinDistance);
            _spawnMaxDistance = Mathf.Max(_spawnMinDistance, _spawnMaxDistance);
            _visibleSpawnMinDistance = Mathf.Clamp(_visibleSpawnMinDistance, _spawnMinDistance, _spawnMaxDistance);
            _despawnDistance = Mathf.Max(_spawnMaxDistance + 1f, _despawnDistance);
        }

        private void OnDrawGizmos()
        {
            if (!_debugDrawSphere) return;

            Vector3 center = transform.position;

            Gizmos.color = _debugSpawnMinColor;
            Gizmos.DrawWireSphere(center, _spawnMinDistance);

            Gizmos.color = _debugVisibleSpawnMinColor;
            Gizmos.DrawWireSphere(center, _visibleSpawnMinDistance);

            Gizmos.color = _debugSpawnMaxColor;
            Gizmos.DrawWireSphere(center, _spawnMaxDistance);

            Gizmos.color = _debugDespawnColor;
            Gizmos.DrawWireSphere(center, _despawnDistance);
        }
    }
}