using Game.Utils;
using RoadSystem;
using UnityEngine;

namespace Game.Systems.CollectibleSystem
{
    public class CollectibleManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Collectible[] _collectibles;
        [SerializeField] private RoadNetwork _roadNetwork;

        [Header("Settings")]
        [SerializeField] private float _spawnIntervalInSeconds = 10f;
        [SerializeField] private int _collectibleLimit = 10;

        private readonly CollectibleSpawner _collectibleSpawner = new();
        private bool _isInitialized = false;
        private float _nextSpawnTime = 0f;

        public void Init()
        {
            if (_isInitialized) return;

            if (_collectibleSpawner == null)
            {
                LogX.LogError("CollectibleSpawner dependency is not assigned in CollectibleManager.", this);

                return;
            }

            _collectibleSpawner.Init(
                _roadNetwork, 
                _collectibles, 
                _collectibleLimit, 
                transform);

            _isInitialized = true;
        }

        public void SpawnCollectibles()
        {
            _collectibleSpawner.SpawnCollectibles();

            _nextSpawnTime = Time.time + _spawnIntervalInSeconds;
        }

        private void Update()
        {
            if (!_isInitialized) return;

            if (Time.time >= _nextSpawnTime)
            {
                SpawnCollectibles();
            }
        }
    }
}
