using Game.Utils;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Systems.PedestrianSystem.Pooling
{
    public class PedestrianPool
    {
        public PedestrianPool(PedestrianPoolProfile pedestrianPoolProfile, int poolSize, Transform parentTransform)
        {
            _pedestrianPrefabs = pedestrianPoolProfile.PedestrianPrefabs;
            _initialPoolSize = poolSize;
            _parentTransform = parentTransform;
        }

        private readonly Pedestrian[] _pedestrianPrefabs;
        private readonly int _initialPoolSize;
        private readonly Transform _parentTransform;
        private readonly List<Pedestrian> _pedestrianPool = new();

        private bool _hasInitialized = false;

        public void Init()
        {
            if (_hasInitialized)
            {
                LogX.LogWarning("Pedestrian pool is already initialized.");

                return;
            }

            if (_pedestrianPrefabs.Length == 0)
            {
                LogX.LogError("No pedestrians assigned in the inspector.");

                return;
            }

            for (int i = 0; i < _initialPoolSize; i++)
            {
                Pedestrian pedestrianPrefab = GetRandomPedestrianPrefab();

                // Instantiate and pool the pedestrian
                GameObject pedestrianInstance = GameObject.Instantiate(
                    pedestrianPrefab.gameObject,
                    _parentTransform);

                pedestrianInstance.SetActive(false);

                Pedestrian pedestrianComponent = pedestrianInstance.GetComponent<Pedestrian>();
                _pedestrianPool.Add(pedestrianComponent);
            }

            _hasInitialized = true;
        }

        public Pedestrian Get()
        {
            if (!_hasInitialized)
            {
                LogX.LogError("Pedestrian pool is not initialized. Call Init() before getting pedestrians.");

                return null;
            }

            if (_pedestrianPool.Count == 0)
            {
                LogX.LogError("Pedestrian pool is not initialized or empty.");

                return null;
            }

            for (int i = 0; i < _pedestrianPool.Count; i++)
            {
                if (!_pedestrianPool[i].gameObject.activeInHierarchy)
                {
                    var pedestrian = _pedestrianPool[i];

                    pedestrian.gameObject.SetActive(true);

                    return pedestrian;
                }
            }

            return null;
        }

        public void Return(Pedestrian pedestrian)
        {
            if (pedestrian == null)
            {
                LogX.LogError("Cannot return a null pedestrian to the pool.");

                return;
            }

            if (_pedestrianPool.Count == 0)
            {
                LogX.LogError("Pedestrian pool is not initialized or empty.");
                return;
            }

            if (!_pedestrianPool.Contains(pedestrian))
            {
                LogX.LogError("The pedestrian being returned does not belong to this pool.");
                return;
            }

            pedestrian.Reset();
            pedestrian.gameObject.SetActive(false);
        }

        private Pedestrian GetRandomPedestrianPrefab()
        {
            if (_pedestrianPrefabs == null || _pedestrianPrefabs.Length == 0)
            {
                LogX.LogError("No pedestrian prefabs available to select from.");

                return null;
            }

            int randomIndex = Random.Range(0, _pedestrianPrefabs.Length);

            return _pedestrianPrefabs[randomIndex];
        }
    }
}
