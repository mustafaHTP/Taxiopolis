using Game.Systems.PedestrianSystem.Pooling;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Systems.PedestrianSystem
{
    /// <summary>
    /// Does what it is told: spawns pedestrians at given points and despawns given pedestrians.
    /// It makes no decisions about where or how many. That is the sim sphere's job.
    /// </summary>
    public class PedestrianSpawner
    {
        private float _minPedestrianSpeed = 0.5f;
        private float _maxPedestrianSpeed = 1.3f;

        private bool _isInitialized = false;
        private readonly List<Pedestrian> _activePedestrians = new();
        private PedestrianPool _pedestrianPool;
        private Transform _spawnRoot;

        public IReadOnlyList<Pedestrian> ActivePedestrians => _activePedestrians;

        public void Init(PedestrianSystemSettings pedestrianSystemSettings, Transform spawnRoot)
        {
            if (_isInitialized)
            {
                LogX.LogWarning("PedestrianSpawner has already been initialized.");

                return;
            }

            if (pedestrianSystemSettings == null)
            {
                LogX.LogError("Pedestrian system settings is null");

                return;
            }

            _minPedestrianSpeed = pedestrianSystemSettings.MinPedestrianSpeed;
            _maxPedestrianSpeed = pedestrianSystemSettings.MaxPedestrianSpeed;
            _spawnRoot = spawnRoot;

            _pedestrianPool = new PedestrianPool(
                pedestrianSystemSettings.PedestrianPoolProfile,
                pedestrianSystemSettings.PedestrianPoolSize,
                _spawnRoot);

            _pedestrianPool.Init();

            _isInitialized = true;
        }

        /// <summary>
        /// Tries to spawn pedestrians at the given points, in list order.
        /// It does NOT guarantee that <paramref name="spawnCount"/> pedestrians are spawned.
        /// A point is skipped while its spawn cooldown is active, and spawning stops
        /// for the remaining points when the pool has no free pedestrian.
        /// </summary>
        /// <param name="points">Candidate points. Shuffle them beforehand if you want random placement.</param>
        /// <param name="spawnCount">Maximum number of pedestrians to spawn.</param>
        public void SpawnPedestrians(IReadOnlyList<PedestrianPoint> points, int spawnCount)
        {
            if (!_isInitialized) return;
            if (points == null || points.Count == 0) return;
            if (spawnCount <= 0) return;

            int spawned = 0;
            for (int i = 0; i < points.Count && spawned < spawnCount; i++)
            {
                SpawnResult result = TrySpawnAt(points[i]);

                if (result == SpawnResult.Spawned)
                {
                    spawned++;
                }
                else if (result == SpawnResult.PoolExhausted)
                {
                    // No free pedestrian, so every remaining point would fail the same way.
                    LogX.LogWarning("Pedestrian pool is exhausted.");
                    break;
                }
            }
        }

        /// <summary>Tries to spawn one pedestrian at every given point.</summary>
        public void SpawnPedestrians(IReadOnlyList<PedestrianPoint> points)
        {
            SpawnPedestrians(points, int.MaxValue);
        }

        public void DespawnPedestrians(IReadOnlyList<Pedestrian> pedestriansToDespawn)
        {
            if (!_isInitialized) return;
            if (pedestriansToDespawn == null || pedestriansToDespawn.Count == 0) return;

            for (int i = 0; i < pedestriansToDespawn.Count; i++)
            {
                Pedestrian pedestrian = pedestriansToDespawn[i];

                // Remove returns false if it was not tracked, so no separate Contains needed.
                if (_activePedestrians.Remove(pedestrian))
                {
                    _pedestrianPool.Return(pedestrian);
                }
                else
                {
                    LogX.LogWarning("Attempted to despawn a pedestrian that is not tracked as active.");
                }
            }
        }

        private enum SpawnResult
        {
            Spawned,
            OnCooldown,
            PoolExhausted
        }

        private SpawnResult TrySpawnAt(PedestrianPoint point)
        {
            if (Time.time < point.NextSpawnTime) return SpawnResult.OnCooldown;

            Pedestrian pedestrian = _pedestrianPool.Get();
            if (pedestrian == null) return SpawnResult.PoolExhausted;

            pedestrian.MoveSpeed = Random.Range(_minPedestrianSpeed, _maxPedestrianSpeed);
            pedestrian.transform.SetPositionAndRotation(
                point.transform.position,
                Quaternion.LookRotation(point.transform.forward));
            pedestrian.SetWaypoint(point);

            // Lets the point start its spawn cooldown.
            point.InformSpawn();

            _activePedestrians.Add(pedestrian);

            return SpawnResult.Spawned;
        }
    }
}