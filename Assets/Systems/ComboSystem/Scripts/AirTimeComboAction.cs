using Game.Car;
using UnityEngine;

namespace Game.Systems.ComboSystem
{
    public class AirTimeComboAction : BaseComboAction
    {
        [Header("Dependencies")]
        [SerializeField] private CarController _carController;

        [Header("Settings")]
        [SerializeField] private float _airTimeForCombo;
        [SerializeField] private float _minSpeedForAirTime;

        private float _airTimer;

        private void Update()
        {
            if (IsAirTimeValid())
            {
                _airTimer += Time.deltaTime;
            }
            else
            {
                _airTimer = 0;
            }

            if (_airTimer >= _airTimeForCombo)
            {
                Success();
                // after success reset timer
                _airTimer = 0;
            }
        }

        private bool IsAirTimeValid()
        {
            if (!_carController.AreAllWheelsAirborne()) return false;
            // Speed is calculated using the car's forward velocity.
            // During a jump, the car may be moving vertically or sideways,
            // causing the forward speed to be low even though the car is moving fast.
            if (_carController.CarState.RbSpeed < _minSpeedForAirTime) return false;

            //* Env check must be enabled in CarController
            CarEnvCheckResultContainer carEnvCheckResultContainer = _carController.CarState.CarEnvCheckResultContainer;
            if (carEnvCheckResultContainer.Front.IsHit
                || carEnvCheckResultContainer.Back.IsHit
                || carEnvCheckResultContainer.Up.IsHit
                || carEnvCheckResultContainer.Down.IsHit
                || carEnvCheckResultContainer.Left.IsHit
                || carEnvCheckResultContainer.Right.IsHit) return false;

            return true;
        }
    }

}