using System;
using Game.Car;
using UnityEngine;

namespace Game.Systems.ComboSystem
{
    public class NearMissComboAction : BaseComboAction
    {
        [SerializeField] private float _minSpeedForNearMiss;
        [SerializeField]
        [Tooltip("Disables near miss checks for a short time after an impact")]
        private float _freezeTimeOnImpact;
        [SerializeField] private NearMissTrigger _leftNearMissTrigger;
        [SerializeField] private NearMissTrigger _rightNearMissTrigger;
        [SerializeField] private CarController _carController;
        [SerializeField] private CarImpactHandler _carImpactHandler;

        private float _nearMissDisabledUntil;

        public override void Init()
        {
            if (_carController == null) return;
            if (_carImpactHandler == null) return;
            if (_leftNearMissTrigger == null) return;
            if (_rightNearMissTrigger == null) return;

            _leftNearMissTrigger.OnVehicleExited += LeftNearMissTrigger_OnVehicleExited;
            _rightNearMissTrigger.OnVehicleExited += RightNearMissTrigger_OnVehicleExited;
            _carImpactHandler.OnImpactDetected += CarImpactHandler_OnImpactDetected;
        }

        public override void Dispose()
        {
            _leftNearMissTrigger.OnVehicleExited -= LeftNearMissTrigger_OnVehicleExited;
            _rightNearMissTrigger.OnVehicleExited -= RightNearMissTrigger_OnVehicleExited;
            _carImpactHandler.OnImpactDetected -= CarImpactHandler_OnImpactDetected;
        }

        private void CarImpactHandler_OnImpactDetected(CarImpactHandler.ImpactInfo _)
        {
            _nearMissDisabledUntil = Time.time + _freezeTimeOnImpact;
        }

        private bool IsNearMissSuccessful()
        {
            if (_carController.CarState.Speed < _minSpeedForNearMiss)
            {
                return false;
            }

            if (Time.time < _nearMissDisabledUntil)
            {
                return false;
            }

            return true;
        }

        private void LeftNearMissTrigger_OnVehicleExited()
        {
            if (!IsNearMissSuccessful()) return;

            Success();
        }

        private void RightNearMissTrigger_OnVehicleExited()
        {
            if (!IsNearMissSuccessful()) return;

            Success();
        }
    }
}
