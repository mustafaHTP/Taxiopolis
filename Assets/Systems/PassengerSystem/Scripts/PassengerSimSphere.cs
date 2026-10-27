using Game.Systems.PassengerSystem.Passengers;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Systems.PassengerSystem
{
    /// <summary>
    /// Decides WHERE and WHEN passengers should exist around this object
    /// (put it on the player car, or on an empty child of it).
    /// It only checks conditions and tells the spawner what to do. The spawner does the work.
    ///
    /// Distances from this object, in order:
    ///   spawnMin <= visibleSpawnMin <= spawnMax < despawn
    ///
    /// A waiting point can spawn a passenger when it is free, inside the spawn ring
    /// (spawnMin..spawnMax), and either outside the camera view or at least visibleSpawnMin away.
    /// Passengers beyond despawn are returned to the pool.
    /// Passengers riding in the car are released from the simulation by the spawner and never seen here.
    /// </summary>
    public class PassengerSimSphere : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _debugDrawSimSphere;
        [SerializeField] private Color _debugSpawnMinColor = Color.red;
        [SerializeField] private Color _debugVisibleSpawnMinColor = Color.cyan;
        [SerializeField] private Color _debugSpawnMaxColor = Color.green;
        [SerializeField] private Color _debugDespawnColor = Color.yellow;

        [Header("Settings")]
        [SerializeField] private bool _isSimulationEnabled = false;

        [Header("Distances")]
        [FormerlySerializedAs("_deadZoneDistance")]
        [Tooltip("Nothing spawns closer than this.")]
        [SerializeField] private float _spawnMinDistance = 20f;

        [Tooltip("Points INSIDE the camera view must be at least this far to spawn. " +
                 "Points outside the view can spawn from Spawn Min. " +
                 "Top-down camera: keep close to Spawn Min. Chase camera: raise it.")]
        [SerializeField] private float _visibleSpawnMinDistance = 35f;

        [Tooltip("Nothing spawns farther than this.")]
        [SerializeField] private float _spawnMaxDistance = 45f;

        [FormerlySerializedAs("_lifeZoneDistance")]
        [Tooltip("Passengers farther than this are despawned. Keep it clearly above Spawn Max, " +
                 "otherwise passengers are despawned right after spawning.")]
        [SerializeField] private float _despawnDistance = 55f;

        // Both values come from PassengerSystemSettings in Init.
        private int _activePassengerInsideSphereLimit;
        private float _simInterval;

        // Reused every tick so the simulation does not allocate.
        private readonly List<PassengerWaitingPoint> _nearbyPoints = new();
        private readonly List<PassengerWaitingPoint> _spawnCandidates = new();
        private readonly List<Passenger> _passengersToDespawn = new();
        private readonly Plane[] _frustumPlanes = new Plane[6];

        private PassengerPointRegistry _passengerPointRegistry;
        private PassengerSpawner _passengerSpawner;
        private bool _isInitialized = false;
        private float _nextSimTime;

        public void Init(
            PassengerSystemSettings passengerSystemSettings, 
            PassengerPointRegistry passengerPointRegistry, 
            PassengerSpawner passengerSpawner)
        {
            if (_isInitialized)
            {
                LogX.LogWarning("PassengerSimSphere has already been initialized.", this);

                return;
            }

            _passengerPointRegistry = passengerPointRegistry;
            _passengerSpawner = passengerSpawner;
            _simInterval = passengerSystemSettings.SimInterval;
            _activePassengerInsideSphereLimit = passengerSystemSettings.ActivePassengerInsideSphereLimit;

            _isInitialized = true;
        }

        public void EnablePassengerSimulation()
        {
            if (!_isInitialized)
            {
                LogX.LogWarning("PassengerSimSphere is not initialized. Call Init first.", this);

                return;
            }

            _isSimulationEnabled = true;
        }

        public void DisablePassengerSimulation()
        {
            _isSimulationEnabled = false;
        }

        private Vector3 GetCenter() => transform.position;

        private void Update()
        {
            if (!_isInitialized) return;
            if (Time.time < _nextSimTime) return;

            _nextSimTime = Time.time + _simInterval;

            float despawnSqr = _despawnDistance * _despawnDistance;

            // One pass over the active passengers:
            // count the waiting ones inside the despawn distance, collect the ones outside.
            _passengersToDespawn.Clear();
            int insideCount = 0;
            IReadOnlyList<Passenger> active = _passengerSpawner.ActivePassengers;
            for (int i = 0; i < active.Count; i++)
            {
                Passenger passenger = active[i];
                float distSqr = (passenger.transform.position - GetCenter()).sqrMagnitude;

                if (distSqr > despawnSqr)
                {
                    _passengersToDespawn.Add(passenger);
                }
                else
                {
                    insideCount++;
                }
            }

            // DESPAWN: always runs, even at full capacity or when the simulation is disabled.
            _passengerSpawner.DespawnPassengers(_passengersToDespawn);

            if (!_isSimulationEnabled) return;

            // SPAWN: only if there is room left.
            int availableSpots = _activePassengerInsideSphereLimit - insideCount;
            if (availableSpots <= 0) return;

            CollectSpawnCandidates(GetCenter());
            if (_spawnCandidates.Count == 0) return;

            // Shuffle so the spawner does not always pick the same points first.
            Shuffle(_spawnCandidates);
            _passengerSpawner.SpawnPassengers(_spawnCandidates, availableSpots);
        }

        /// <summary>
        /// Fills _spawnCandidates with every FREE waiting point that may spawn a passenger now.
        /// Occupied points are excluded here, otherwise they would use up the spawn budget
        /// and the spawner would reject them.
        /// </summary>
        private void CollectSpawnCandidates(Vector3 center)
        {
            _spawnCandidates.Clear();

            Camera cam = Camera.main;
            if (cam == null) return;

            // Computed once per tick instead of once per point.
            GeometryUtility.CalculateFrustumPlanes(cam, _frustumPlanes);

            float minSqr = _spawnMinDistance * _spawnMinDistance;
            float maxSqr = _spawnMaxDistance * _spawnMaxDistance;
            float visibleMinSqr = _visibleSpawnMinDistance * _visibleSpawnMinDistance;

            // The registry's grid removes points that are obviously too far away.
            // The exact checks below still decide.
            _passengerPointRegistry.QueryNearbyPoints(center, _spawnMaxDistance, _nearbyPoints);

            for (int i = 0; i < _nearbyPoints.Count; i++)
            {
                PassengerWaitingPoint point = _nearbyPoints[i];
                Vector3 pointPosition = point.transform.position;
                float distSqr = (pointPosition - center).sqrMagnitude;

                if (distSqr <= minSqr || distSqr > maxSqr) continue;
                if (_passengerSpawner.IsWaitingPointOccupied(point)) continue;

                // Far enough: accepted whether visible or not, so the camera test is skipped.
                // Only closer points must be outside the camera view.
                if (distSqr < visibleMinSqr
                    && CameraUtils.IsPointInFrustum(_frustumPlanes, pointPosition))
                {
                    continue;
                }

                _spawnCandidates.Add(point);
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
            if (!_debugDrawSimSphere) return;

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