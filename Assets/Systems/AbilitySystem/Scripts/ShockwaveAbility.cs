using Framework.CustomEventBus;
using Game.Car;
using Game.Utils;
using Game.VFX;
using UnityEngine;

namespace Game.Systems.AbilitySystem
{
    public class ShockwaveAbility : BaseAbility
    {
        [Header("Debug")]
        [SerializeField] private bool _debugDrawShockwaveSphere;
        [SerializeField] private Color _shockwaveSphereColor = Color.red;

        [Header("Settings")]
        [SerializeField] private LayerMask _affectedLayers;
        [SerializeField] private float _shockwaveRadius = 5f;
        [SerializeField] private float _shockwaveForce = 500f;
        [SerializeField] private float _upwardsModifier = 15f;

        [Header("Dependencies")]
        [SerializeField] private ParticleSystem _shockwaveVfx;
        [SerializeField] private CarVfxHandler _carVfxHandler;
        [SerializeField] private CarSfxHandler _carSfxHandler;
        [SerializeField] private Transform _carTransform;

        public override void Init()
        {
            if (_carTransform == null)
            {
                LogX.LogError("ShockwaveAbility Init failed: Car Transform not found!");

                return;
            }

            if (_carVfxHandler == null)
            {
                LogX.LogError("ShockwaveAbility Init failed: CarVfxHandler component not found!");

                return;
            }

            if (_carSfxHandler == null)
            {
                LogX.LogError("ShockwaveAbility Init failed: CarSfxHandler component not found!");

                return;
            }

            ChangeState(AbilityState.Ready);
        }

        public override void OnButtonPressed()
        {
            switch (State)
            {
                case AbilityState.Ready:
                    Activate();
                    _cooldownEndTime = Time.time + CooldownDuration;
                    OnCooldownStarted?.Invoke();
                    ChangeState(AbilityState.Cooldown);
                    break;
            }
        }

        public override void Tick()
        {
            switch (State)
            {
                case AbilityState.Cooldown:
                    if (Time.time > _cooldownEndTime)
                    {
                        OnCooldownFinished?.Invoke();
                        ChangeState(AbilityState.Ready);
                    }
                    break;
            }
        }

        private void Activate()
        {
            Collider[] colliders =
                Physics.OverlapSphere(
                    _carTransform.position,
                     _shockwaveRadius,
                     _affectedLayers);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].attachedRigidbody == null) continue;

                colliders[i].attachedRigidbody.AddExplosionForce(
                    _shockwaveForce,
                    _carTransform.position,
                    _shockwaveRadius,
                    _upwardsModifier,
                    ForceMode.VelocityChange);
            }

            _shockwaveVfx.Play();
            _carSfxHandler.PlayShockwaveSfx();

            OnActivation?.Invoke();
        }

        private void OnDrawGizmos()
        {
            if (_debugDrawShockwaveSphere && _carTransform != null)
            {
                Gizmos.color = _shockwaveSphereColor;
                Gizmos.DrawWireSphere(_carTransform.position, _shockwaveRadius);
            }
        }
    }
}
