using Game.Systems.PassengerSystem.Passengers;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems.PassengerSystem.Pooling
{
    public class PassengerPool
    {
        public PassengerPool(PassengerPoolProfile passengerPoolProfile, int poolSize, Transform parentTransform)
        {
            _passengerPrefabs = passengerPoolProfile.PassengerPrefabs;
            _poolSize = poolSize;
            _parentTransform = parentTransform;

            _passengerPool = new();
        }

        private readonly int _poolSize;
        private readonly Transform _parentTransform;
        private readonly Passenger[] _passengerPrefabs;
        private readonly List<Passenger> _passengerPool;

        private bool _isInitialized = false;

        public void Init()
        {
            if (_isInitialized) return;

            if (_passengerPrefabs == null || _passengerPrefabs.Length == 0)
            {
                LogX.LogError("No passenger prefabs assigned in the inspector.");
                return;
            }

            if (_poolSize <= 0)
            {
                LogX.LogError("Pool size must be greater than zero.");

                return;
            }

            if (_parentTransform == null)
            {
                LogX.LogError("Parent transform is not assigned.");

                return;
            }

            for (int i = 0; i < _poolSize; i++)
            {
                Passenger passengerPrefab = GetRandomPassengerPrefab();
                Passenger passengerInstance = GameObject.Instantiate(passengerPrefab, _parentTransform);
                passengerInstance.PassengerMovementHandler.DisableAgent();
                passengerInstance.gameObject.SetActive(false);

                _passengerPool.Add(passengerInstance);
            }

            _isInitialized = true;
        }

        public Passenger Get()
        {
            if (!_isInitialized)
            {
                LogX.LogError("PassengerPool is not initialized. Call Init() before using the pool.");
                return null;
            }

            for (int i = 0; i < _passengerPool.Count; i++)
            {
                var passenger = _passengerPool[i];

                // activeSelf, not activeInHierarchy: a passenger riding in the car is a child of the car,
                // so if the car object is ever disabled, activeInHierarchy would wrongly report
                // the rider as "free" and the pool would hand it out again.
                if (!passenger.gameObject.activeSelf)
                {
                    passenger.gameObject.SetActive(true);

                    return passenger;
                }
            }

            LogX.LogWarning("No available passengers in the pool. Consider increasing the pool size.");

            return null;
        }

        public void Return(Passenger passenger)
        {
            if (passenger == null)
            {
                LogX.LogError("Cannot return a null passenger to the pool.");
                return;
            }

            if (!_passengerPool.Contains(passenger))
            {
                LogX.LogError("The passenger being returned does not belong to this pool.");
                return;
            }

            if (!passenger.gameObject.activeSelf)
            {
                LogX.LogWarning("The passenger is already in the pool.");
                return;
            }

            passenger.ResetState();
            passenger.PassengerMovementHandler.DisableAgent();

            // A passenger can come back unparented (dropped off / angry) or still inside the car,
            // possibly with its meshes hidden (picked up). Restore the pooled defaults.
            passenger.transform.SetParent(_parentTransform);
            if (passenger.MeshesGameObject != null)
            {
                passenger.MeshesGameObject.SetActive(true);
            }

            passenger.gameObject.SetActive(false);
        }

        private Passenger GetRandomPassengerPrefab()
        {
            if (_passengerPrefabs == null || _passengerPrefabs.Length == 0)
            {
                LogX.LogError("No passenger prefabs assigned in the inspector.");

                return null;
            }

            return _passengerPrefabs[Random.Range(0, _passengerPrefabs.Length)];
        }
    }
}