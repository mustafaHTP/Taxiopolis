using System;
using System.Collections;
using Game.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Systems.TrafficSystem.Imposter
{
    [RequireComponent(typeof(SplineContainer))]
    public class ImposterTrafficRoad : MonoBehaviour
    {
        [SerializeField] private ImposterTrafficVehicle[] _imposterTrafficVehicles;
        [SerializeField] private float _spawnInterval;

        private SplineContainer _splineContainer;
        private Coroutine _startTrafficCoroutine;
        private ImposterTrafficVehicle[] _spawnedVehicles;

        public void StartTraffic()
        {
            if (_splineContainer == null)
            {
                LogX.LogWarning($"{nameof(SplineContainer)} is not set!");

                return;
            }

            if (_imposterTrafficVehicles == null)
            {
                LogX.LogWarning($"Traffic vehicles is not set!");

                return;
            }

            if (_spawnInterval < 0)
            {
                LogX.LogWarning($"Spawn interval is smaller than 0!");

                return;
            }

            if (_spawnedVehicles != null)
            {
                ClearTraffic();
            }

            // init array for spawned vehicles
            _spawnedVehicles = new ImposterTrafficVehicle[_imposterTrafficVehicles.Length];

            // Spawn location is beginning of spline
            Spline spline = _splineContainer.Splines[0];
            float3 beginningOfSpline = SplineUtility.EvaluatePosition(spline, 0f);
            Vector3 worldSpaceBeginningOfSpline = transform.TransformPoint(beginningOfSpline);

            _startTrafficCoroutine = StartCoroutine(StartTrafficCoroutine(worldSpaceBeginningOfSpline));
        }

        public void ClearTraffic()
        {
            if (_startTrafficCoroutine != null) StopCoroutine(_startTrafficCoroutine);

            // Despawn vehicles
            if (_spawnedVehicles == null || _spawnedVehicles.Length == 0) return;

            for (int i = 0; i < _spawnedVehicles.Length; i++)
            {
                Destroy(_spawnedVehicles[i]);
            }

            // After despawn clear array
            _spawnedVehicles = null;
        }

        private IEnumerator StartTrafficCoroutine(Vector3 worldSpaceBeginningOfSpline)
        {
            for (int i = 0; i < _imposterTrafficVehicles.Length; i++)
            {
                // Spawn vehicle
                ImposterTrafficVehicle imposterTrafficVehicle = _imposterTrafficVehicles[i];
                ImposterTrafficVehicle spawnedVehicle = Instantiate(imposterTrafficVehicle, worldSpaceBeginningOfSpline, Quaternion.identity, transform);

                spawnedVehicle.Init(_splineContainer);
                spawnedVehicle.Play();

                _spawnedVehicles[i] = spawnedVehicle;

                // wait for spawn interval
                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        private void Awake()
        {
            _splineContainer = GetComponent<SplineContainer>();
        }
    }
}