using Framework.CustomEventBus;
using Game.Car;
using Game.Utils;
using Game.VFX;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Systems.AbilitySystem
{
    public class NitroAbility : BaseAbility, IMotorTorqueModifier
    {
        [Header("Settings")]
        [SerializeField] private float _motorTorqueBoost = 500f;
        [SerializeField] private float _nitroTankAmount = 50f;
        [SerializeField] private float _nitroDuration = 5f;
        [SerializeField] private float _nitroRefillDuration = 10f;
        [SerializeField] private float _postProcessTransitionDuration = 0.3f;

        [Header("Dependencies")]
        [SerializeField] private CarController _carController;
        [SerializeField] private CarVfxHandler _carVfxHandler;
        [SerializeField] private CarSfxHandler _carSfxHandler;
        [SerializeField] private Volume _volume;
        [SerializeField] private ParticleSystem _nitroVfx;

        private float _currentNitroAmount;
        private float _targetVolumeWeight;

        public float Modifier => _motorTorqueBoost * (State == AbilityState.Use ? 1f : 0f);

        public override void Init()
        {
            if (_carController == null)
            {
                LogX.LogError("NosAbility Init failed: CarController component not found!");

                return;
            }

            if (_carVfxHandler == null)
            {
                LogX.LogWarning("NosAbility Init failed: CarVfxHandler component not found!");
            }

            if (_carSfxHandler == null)
            {
                LogX.LogWarning("NosAbility Init failed: CarSfxHandler component not found!");
            }

            _carController.RegisterModifier(this);

            _currentNitroAmount = _nitroTankAmount;

            _targetVolumeWeight = 0f;
            if (_volume != null)
            {
                _volume.weight = 0f;
            }

            ChangeState(AbilityState.Ready);
        }

        public override void Dispose()
        {
            if (_carController != null)
            {
                _carController.UnregisterModifier(this);
            }
        }

        public override float GetAmountNormalized()
        {
            return _currentNitroAmount / _nitroTankAmount;
        }

        public override void OnButtonPressed()
        {
            switch (State)
            {
                case AbilityState.Ready:
                    if (CanActivate())
                    {
                        Activate();
                        ChangeState(AbilityState.Use);
                    }
                    break;
            }
        }

        public override void OnButtonReleased()
        {
            switch (State)
            {
                case AbilityState.Use:
                    DeactivateNitro();
                    ChangeState(AbilityState.Ready);
                    break;
            }
        }

        public override void Tick()
        {
            UpdateVolumeWeight();

            switch (State)
            {
                case AbilityState.Ready:
                    if (!HasTankFull())
                    {
                        FillDelta();
                    }
                    break;
                case AbilityState.Cooldown:
                    if (Time.unscaledTime > _cooldownEndTime)
                    {
                        OnCooldownFinished?.Invoke();
                        ChangeState(AbilityState.Ready);
                    }
                    break;
                case AbilityState.Use:
                    ConsumeDelta();
                    if (!HasNitro())
                    {
                        DeactivateNitro();
                        _cooldownEndTime = Time.unscaledTime + CooldownDuration;
                        OnCooldownStarted?.Invoke();
                        ChangeState(AbilityState.Cooldown);
                    }
                    break;
            }
        }

        private void UpdateVolumeWeight()
        {
            if (_volume == null) return;

            // Full 0 -> 1 transition takes _postProcessTransitionDuration seconds
            float speed = _postProcessTransitionDuration > 0f
                ? 1f / _postProcessTransitionDuration
                : float.MaxValue;

            _volume.weight = Mathf.MoveTowards(
                _volume.weight,
                _targetVolumeWeight,
                speed * Time.unscaledDeltaTime);
        }

        private bool HasNitro()
        {
            return _currentNitroAmount > 0f;
        }

        private void Activate()
        {
            _targetVolumeWeight = 1f;

            _nitroVfx.Play();

            _carSfxHandler.PlayNitroStartSfx();
            _carSfxHandler.PlayNitroLoopSfx();

            OnActivation?.Invoke();

            LogX.LogDebug("NOS Activated: Nitro has been activated.");
        }

        private void DeactivateNitro()
        {
            _targetVolumeWeight = 0f;

            _nitroVfx.Stop();

            _carSfxHandler.StopNitroLoopSfx();
            _carSfxHandler.PlayNitroEndSfx();

            OnDeactivation?.Invoke();

            LogX.LogDebug("NOS Deactivated: Nitro has been deactivated");
        }

        private bool CanActivate()
        {
            float nitroConsumedPerSecond = _nitroTankAmount / _nitroDuration;
            float nitroConsumed = nitroConsumedPerSecond * Time.deltaTime;

            if (_currentNitroAmount > nitroConsumed) return true;

            return false;
        }

        private bool HasTankFull()
        {
            return _currentNitroAmount == _nitroTankAmount;
        }

        private void FillDelta()
        {
            float nitroRefilledPerSecond = _nitroTankAmount / _nitroRefillDuration;
            float nitroRefilled = nitroRefilledPerSecond * Time.deltaTime;

            _currentNitroAmount = Mathf.Min(_currentNitroAmount + nitroRefilled, _nitroTankAmount);
        }

        private void ConsumeDelta()
        {
            float nitroConsumedPerSecond = _nitroTankAmount / _nitroDuration;
            float nitroConsumed = nitroConsumedPerSecond * Time.deltaTime;

            _currentNitroAmount = Mathf.Max(_currentNitroAmount - nitroConsumed, 0f);
        }
    }
}