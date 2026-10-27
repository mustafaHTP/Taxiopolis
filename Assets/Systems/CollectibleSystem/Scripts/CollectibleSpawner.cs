using Game.Utils;
using RoadSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems.CollectibleSystem
{
    public class CollectibleSpawner
    {
        private readonly Dictionary<Collectible, RoadPoint> _spawnedCollectibles = new();
        private Collectible[] _collectibles;
        private Transform _spawnRoot;
        private RoadNetwork _roadNetwork;
        private int _collectibleLimit = 10;

        public int ActiveCollectibleCount => _spawnedCollectibles.Count;

        public void Init(RoadNetwork roadNetwork, Collectible[] collectibles, int collectibleLimit, Transform spawnRoot)
        {
            _roadNetwork = roadNetwork;
            _collectibles = collectibles;
            _collectibleLimit = collectibleLimit;
            _spawnRoot = spawnRoot;

            if (_roadNetwork == null)
            {
                LogX.LogError("RoadNetwork dependency is not assigned in CollectibleSpawner.");

                return;
            }

            if (_collectibles == null || _collectibles.Length == 0)
            {
                LogX.LogError("Collectible data list is empty in CollectibleSpawner.");

                return;
            }
        }

        public void SpawnCollectibles()
        {
            if (_spawnedCollectibles.Count >= _collectibleLimit)
            {
                LogX.LogInfo("Collectible limit reached. Cannot spawn more collectibles.");

                return;
            }

            List<RoadPoint> candidateSpawnPoints = new(_roadNetwork.RoadPoints);

            // Remove points that already have collectibles
            foreach (var roadPoint in _spawnedCollectibles.Values)
            {
                candidateSpawnPoints.Remove(roadPoint);
            }

            if (candidateSpawnPoints.Count == 0)
            {
                LogX.LogWarning("No available spawn points for collectibles.");

                return;
            }

            int actualSpawnCount = Mathf.Min(
                _collectibleLimit - _spawnedCollectibles.Count,
                candidateSpawnPoints.Count);

            for (int i = 0; i < actualSpawnCount; i++)
            {
                // Select a random road point
                int randomRoadPointIndex = Random.Range(0, candidateSpawnPoints.Count);
                RoadPoint spawnPoint = candidateSpawnPoints[randomRoadPointIndex];

                // Select a random collectible
                int randomCollectibleIndex =
                    Random.Range(0, _collectibles.Length);
                Collectible selectedCollectible =
                    _collectibles[randomCollectibleIndex];

                Vector3 spawnPosition = spawnPoint.transform.TransformPoint(selectedCollectible.CollectibleData.SpawnOffset);
                Quaternion spawnRotation = spawnPoint.transform.rotation;
                
                Collectible spawnedCollectible = GameObject.Instantiate(
                    selectedCollectible,
                    spawnPosition,
                    spawnRotation,
                    _spawnRoot);

                _spawnedCollectibles.Add(spawnedCollectible, spawnPoint);
                candidateSpawnPoints.RemoveAt(randomRoadPointIndex);
            }
        }
    }
}