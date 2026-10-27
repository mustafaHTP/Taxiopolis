using Game.Car;
using UnityEngine;

namespace Game.Systems.ComboSystem
{
    public class DriftComboAction : BaseComboAction
    {
        [Header("Dependencies")]
        [SerializeField] private CarController _carController;

        [Header("Settings")]
        [SerializeField] private float _driftTimeToCombo;

        private bool _isDrifting;
        private float _driftTimer;

        public override void Init()
        {
            if (_carController == null) return;

            _carController.OnDriftStarted += CarController_OnDriftStarted;
            _carController.OnDriftEnded += CarController_OnDriftEnded;
        }

        public override void Dispose()
        {
            _carController.OnDriftStarted -= CarController_OnDriftStarted;
            _carController.OnDriftEnded -= CarController_OnDriftEnded;
        }

        private void Update()
        {
            if (_isDrifting)
            {
                _driftTimer += Time.deltaTime;

                if (_driftTimer >= _driftTimeToCombo)
                {
                    Success();
                    // after success reset timer
                    _driftTimer = 0;
                }
            }
            else
            {
                _driftTimer = 0;
            }
        }

        private void CarController_OnDriftEnded()
        {
            _isDrifting = false;
        }

        private void CarController_OnDriftStarted()
        {
            _isDrifting = true;
        }
    }
}
