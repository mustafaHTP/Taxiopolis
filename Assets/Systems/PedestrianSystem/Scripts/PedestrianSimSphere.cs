using Game.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Systems.PedestrianSystem
{
    /// <summary>
    /// Decides WHERE and WHEN pedestrians should exist around this object
    /// (put it on the player car, or on an empty child of it).
    /// It only checks conditions and tells the spawner what to do. The spawner does the work.
    ///
    /// Distances from this object, in order:
    ///   spawnMin <= visibleSpawnMin <= spawnMax < despawn
    ///
    /// A point can spawn a pedestrian when it is inside the spawn ring (spawnMin..spawnMax)
    /// and either it is outside the camera view, or it is at least visibleSpawnMin away
    /// (far enough that popping in is not noticeable).
    /// Pedestrians farther than despawn are returned to the pool.
    /// </summary>
    public class PedestrianSimSphere : MonoBehaviour
    {
        [Header("Gizmos Settings")]
        [SerializeField] private bool _debugDrawSphere = true;
        [SerializeField] private Color _debugSpawnMinColor = Color.red;
        [SerializeField] private Color _debugVisibleSpawnMinColor = Color.cyan;
        [SerializeField] private Color _debugSpawnMaxColor = Color.green;
        [SerializeField] private Color _debugDespawnColor = Color.yellow;

        [Header("General Settings")]
        [SerializeField] private bool _enableSimulation = false;

        [Header("Distances")]
        [FormerlySerializedAs("_deadZoneDistance")]
        [Tooltip("Nothing spawns closer than this, so the player never sees pedestrians appear right next to the car.")]
        [SerializeField] private float _spawnMinDistance = 20f;

        [Tooltip("Points INSIDE the camera view must be at least this far to spawn. " +
                 "Points outside the view can spawn from Spawn Min. " +
                 "Top-down camera: keep close to Spawn Min. Chase camera: raise it.")]
        [SerializeField] private float _visibleSpawnMinDistance = 35f;

        [Tooltip("Nothing spawns farther than this.")]
        [SerializeField] private float _spawnMaxDistance = 45f;

        [FormerlySerializedAs("_lifeZoneDistance")]
        [Tooltip("Pedestrians farther than this are despawned. Keep it clearly above Spawn Max, " +
                 "otherwise pedestrians are despawned right after spawning.")]
        [SerializeField] private float _despawnDistance = 55f;

        // Both values come from PedestrianSystemSettings in Init.
        private int _activePedestrianLimitInSimSphere;
        private float _pedestrianSimInterval;

        // Reused every tick so the simulation does not allocate.
        private readonly List<Pedestrian> _pedestriansOutsideDespawnDistance = new();
        private readonly List<PedestrianPoint> _nearbyPoints = new();
        private readonly List<PedestrianPoint> _spawnCandidates = new();
        private readonly Plane[] _frustumPlanes = new Plane[6];

        private bool _isInitialized = false;
        private float _nextPedestrianSimTime = 0f;
        private PedestrianSpawner _pedestrianSpawner;
        private PedestrianPointRegistry _pedestrianPointRegistry;

        public void Init(
            PedestrianSystemSettings pedestrianSystemSettings, 
            PedestrianPointRegistry pedestrianPointRegistry,
            PedestrianSpawner pedestrianSpawner)
        {
            if (_isInitialized)
            {
                LogX.LogWarning("PedestrianSimSphere has already been initialized.", this);

                return;
            }

            if (pedestrianSystemSettings == null)
            {
                LogX.LogError("Pedestrian system settings is null", this);

                return;
            }

            _pedestrianPointRegistry = pedestrianPointRegistry;
            _pedestrianSpawner = pedestrianSpawner;
            _pedestrianSimInterval = pedestrianSystemSettings.PedestrianSimInterval;
            _activePedestrianLimitInSimSphere = pedestrianSystemSettings.ActivePedestrianLimitInSimSphere;

            _isInitialized = true;
        }

        public void EnablePedestrianSimulation()
        {
            _enableSimulation = true;
        }

        public void DisablePedestrianSimulation()
        {
            _enableSimulation = false;
        }

        /// <summary>
        /// Lets the camera system adapt to the current camera mode at runtime
        /// (for example a smaller value for top-down, a larger one for chase).
        /// </summary>
        public void SetVisibleSpawnMinDistance(float distance)
        {
            _visibleSpawnMinDistance = Mathf.Clamp(distance, _spawnMinDistance, _spawnMaxDistance);
        }

        private void Update()
        {
            if (!_isInitialized) return;
            if (Time.time < _nextPedestrianSimTime) return;

            _nextPedestrianSimTime = Time.time + _pedestrianSimInterval;

            Vector3 center = transform.position;
            float despawnSqr = _despawnDistance * _despawnDistance;

            // One pass over the active pedestrians:
            // count the ones inside the despawn distance, collect the ones outside.
            _pedestriansOutsideDespawnDistance.Clear();
            int insideCount = 0;
            IReadOnlyList<Pedestrian> active = _pedestrianSpawner.ActivePedestrians;
            for (int i = 0; i < active.Count; i++)
            {
                float distSqr = (active[i].transform.position - center).sqrMagnitude;
                if (distSqr > despawnSqr)
                {
                    _pedestriansOutsideDespawnDistance.Add(active[i]);
                }
                else
                {
                    insideCount++;
                }
            }

            // DESPAWN: always runs, even at full capacity or when the simulation is disabled.
            _pedestrianSpawner.DespawnPedestrians(_pedestriansOutsideDespawnDistance);

            if (!_enableSimulation) return;

            // SPAWN: only if there is room left.
            int spawnCount = _activePedestrianLimitInSimSphere - insideCount;
            if (spawnCount <= 0) return;

            CollectSpawnCandidates(center);
            if (_spawnCandidates.Count == 0) return;

            // Shuffle so the spawner does not always pick the same points first.
            Shuffle(_spawnCandidates);
            _pedestrianSpawner.SpawnPedestrians(_spawnCandidates, spawnCount);
        }

        /// <summary>
        /// Fills _spawnCandidates with every point that may spawn a pedestrian right now:
        /// inside the spawn ring, and either outside the camera view or far enough away.
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
            _pedestrianPointRegistry.QueryNearbyPoints(center, _spawnMaxDistance, _nearbyPoints);

            for (int i = 0; i < _nearbyPoints.Count; i++)
            {
                PedestrianPoint point = _nearbyPoints[i];
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