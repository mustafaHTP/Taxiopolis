using Game.Car;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems.TrafficSystem.Pooling
{
    public class VehiclePool
    {
        public VehiclePool(VehiclePoolProfile vehiclePoolProfile, int poolSize, Transform parentTransform)
        {
            _vehicleSpawnEntries = vehiclePoolProfile.VehicleSpawnEntries;
            _initialPoolSize = poolSize;
            _trafficManager = parentTransform;
        }

        private readonly VehicleSpawnEntry[] _vehicleSpawnEntries;
        private readonly int _initialPoolSize;
        private readonly Transform _trafficManager;
        private readonly List<GameObject> _vehiclePool = new();

        private bool _hasInitialized = false;

        public void Init()
        {
            if (_hasInitialized)
            {
                LogX.LogWarning("Vehicle pool is already initialized.");
                return;
            }

            if (_vehicleSpawnEntries == null || _vehicleSpawnEntries.Length == 0)
            {
                LogX.LogError("No traffic vehicles assigned in the inspector.");
                return;
            }

            for (int i = 0; i < _initialPoolSize; i++)
            {
                GameObject vehiclePrefab = GetRandomVehicle();
                if (vehiclePrefab != null)
                {
                    // Instantiate and pool the vehicle
                    GameObject vehicleInstance = GameObject.Instantiate(
                        vehiclePrefab,
                        _trafficManager.transform);

                    if (!vehicleInstance.TryGetComponent(out CarController carController))
                    {
                        LogX.LogError("The instantiated vehicle does not have a CarController component.");

                        GameObject.Destroy(vehicleInstance);

                        continue;
                    }

                    if (!vehicleInstance.TryGetComponent(out CarColliderHandler carColliderHandler))
                    {
                        LogX.LogError("The instantiated vehicle does not have a CarColliderHandler component.");

                        GameObject.Destroy(vehicleInstance);

                        continue;
                    }

                    SetPooledState(carController, carColliderHandler);
                    vehicleInstance.SetActive(false);
                    _vehiclePool.Add(vehicleInstance);
                }
                else
                {
                    LogX.LogError("Failed to instantiate vehicle prefab.");

                    break;
                }
            }

            LogX.LogInfo($"Vehicle pool initialized with {_vehiclePool.Count} vehicles.");

            _hasInitialized = true;
        }

        public GameObject Get()
        {
            if (!_hasInitialized)
            {
                LogX.LogError("Vehicle pool is not initialized. Call Init() before getting vehicles.");
                return null;
            }

            if (_vehiclePool.Count == 0)
            {
                LogX.LogError("Vehicle pool is not initialized or empty.");
                return null;
            }

            for (int i = 0; i < _vehiclePool.Count; i++)
            {
                // activeSelf, not activeInHierarchy: vehicles are children of the traffic spawner,
                // so if the spawner object is ever disabled, activeInHierarchy would report every
                // pooled vehicle as "free" and the pool would hand out cars that are in use.
                if (!_vehiclePool[i].activeSelf)
                {
                    var vehicle = _vehiclePool[i];

                    EnableVehicleState(vehicle);

                    vehicle.SetActive(true);

                    return vehicle;
                }
            }

            return null;
        }

        public void Return(GameObject vehicle)
        {
            if (vehicle == null)
            {
                LogX.LogError("Cannot return a null vehicle to the pool.");
                return;
            }

            if (_vehiclePool.Count == 0)
            {
                LogX.LogError("Vehicle pool is not initialized or empty.");
                return;
            }

            if (!_vehiclePool.Contains(vehicle))
            {
                LogX.LogError("The vehicle being returned does not belong to this pool.");
                return;
            }

            if (!vehicle.activeSelf)
            {
                LogX.LogWarning("The vehicle is already in the pool.");
                return;
            }

            DisableVehicleState(vehicle);

            vehicle.SetActive(false);
        }

        private void DisableVehicleState(GameObject vehicle)
        {
            if (!vehicle.TryGetComponent(out CarController carController))
            {
                LogX.LogError("The instantiated vehicle does not have a CarController component.");

                return;
            }

            if (!vehicle.TryGetComponent(out CarColliderHandler carColliderHandler))
            {
                LogX.LogError("The instantiated vehicle does not have a CarColliderHandler component.");
                return;
            }

            // This method can be expanded to reset any additional state of the vehicle if needed.
            SetPooledState(carController, carColliderHandler);
        }

        private void EnableVehicleState(GameObject vehicle)
        {
            if (!vehicle.TryGetComponent(out CarController carController))
            {
                LogX.LogError("The instantiated vehicle does not have a CarController component.");
                return;
            }
            if (!vehicle.TryGetComponent(out CarColliderHandler carColliderHandler))
            {
                LogX.LogError("The instantiated vehicle does not have a CarColliderHandler component.");
                return;
            }
            // This method can be expanded to set any additional state of the vehicle if needed.
            carColliderHandler.BodyCollider.enabled = true;
            carController.Rigidbody.isKinematic = false;
        }

        // Same state is applied when a vehicle is created and when it is returned.
        // Velocity is cleared BEFORE the body becomes kinematic (velocity cannot be set on a kinematic body).
        private static void SetPooledState(CarController carController, CarColliderHandler carColliderHandler)
        {
            carColliderHandler.BodyCollider.enabled = false;
            carController.Rigidbody.linearVelocity = Vector3.zero;
            carController.Rigidbody.angularVelocity = Vector3.zero;
            carController.Rigidbody.isKinematic = true;
            carController.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private GameObject GetRandomVehicle()
        {
            if (_vehicleSpawnEntries.Length == 0)
            {
                LogX.LogError("No traffic vehicles assigned in the inspector.");

                return null;
            }

            int randomIndex = Random.Range(0, _vehicleSpawnEntries.Length);

            return _vehicleSpawnEntries[randomIndex].VehiclePrefab;
        }
    }
}