using Framework.CustomEventBus;
using Framework.Services;
using Game.Car;
using Game.Managers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Systems.AbilitySystem
{
    public class SpeedBreakerAbility : BaseAbility
    {
        [Header("Settings")]
        [SerializeField] private float _slowdownDuration = 4f;
        [SerializeField] private float _slowdownTimeScale = 0.5f;
        [SerializeField] private float _postProcessTransitionDuration = 1.0f;

        [Header("Dependencies")]
        [SerializeField] private CarSfxHandler _carSfxHandler;
        [SerializeField] private Volume _volume;

        private TimeManager _timeManager;
        private float _slowdownTimer;
        private float _targetVolumeWeight;

        public override void Init()
        {
            _timeManager = ServiceLocator.Global.GetService<TimeManager>();
            _slowdownTimer = _slowdownDuration;

            _targetVolumeWeight = 0f;
            _volume.weight = 0f;

            ChangeState(AbilityState.Ready);
        }

        public override float GetAmountNormalized()
        {
            return _slowdownTimer / _slowdownDuration;
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
                case AbilityState.Use:
                    Deactivate();
                    ChangeState(AbilityState.Ready);
                    break;
            }
        }

        public override void Tick()
        {
            UpdateVolumeWeight();

            switch (State)
            {
                case AbilityState.Cooldown:
                    if (Time.unscaledTime > _cooldownEndTime)
                    {
                        OnCooldownFinished?.Invoke();
                        ChangeState(AbilityState.Ready);
                    }
                    break;
                case AbilityState.Ready:
                    if (!HasTimerFull())
                    {
                        FillDelta();
                    }
                    break;
                case AbilityState.Use:
                    ConsumeDelta();
                    if (!HasTime())
                    {
                        Deactivate();
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

        private bool HasTime()
        {
            return _slowdownTimer > 0f;
        }

        private bool HasTimerFull() => _slowdownTimer == _slowdownDuration;

        private void ConsumeDelta()
        {
            _slowdownTimer = Mathf.Max(0f, _slowdownTimer - Time.unscaledDeltaTime);
        }

        private void FillDelta()
        {
            float fillDelta = _slowdownDuration / _slowdownDuration * Time.unscaledDeltaTime;
            _slowdownTimer = Mathf.Min(_slowdownDuration, _slowdownTimer + fillDelta);
        }

        private bool CanActivate()
        {
            return _slowdownTimer > 0f;
        }

        private void Activate()
        {
            _timeManager.SetTimeScale(_slowdownTimeScale);
            _targetVolumeWeight = 1f;

            _carSfxHandler.PlaySpeedbreakerStart();
            _carSfxHandler.PlaySpeedbreakerLoop();

            OnActivation?.Invoke();
        }

        private void Deactivate()
        {
            _timeManager.ResetTimeScale();
            _targetVolumeWeight = 0f;

            _carSfxHandler.StopSpeedbreakerLoop();
            _carSfxHandler.PlaySpeedbreakerEnd();

            OnDeactivation?.Invoke();
        }
    }
}