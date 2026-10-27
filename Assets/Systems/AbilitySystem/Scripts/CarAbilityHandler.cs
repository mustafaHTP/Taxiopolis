using Game.Input;
using Game.Systems.AbilitySystem;
using Game.Utils;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Car
{
    public class CarAbilityHandler : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Instead of assigning abilities manually, just give ability holder object and search under its children.")]
        private Transform _abilityHolder;
        [SerializeField] private BaseAbility[] _abilities;

        private ICarInputProvider _carInputProvider;
        private BaseAbility _nitroAbility;
        private BaseAbility _shockwaveAbility;
        private BaseAbility _speedBreakerAbility;

        public BaseAbility GetAbility(AbilityType abilityType)
        {
            if (_abilities == null || _abilities.Length == 0) return null;

            for (int i = 0; i < _abilities.Length; i++)
            {
                if (_abilities[i].AbilityType == abilityType) return _abilities[i];
            }

            return null;
        }

        private void Awake()
        {
            if (!TryGetComponent(out _carInputProvider))
            {
                LogX.LogError("CarAbilityHandler Awake failed: ICarInputProvider component not found on the GameObject.", this);

                return;
            }

            SetupAbilities();
        }

        private void Start()
        {
            SetupAbilityInputs();
        }

        private void Update()
        {
            for (int i = 0; i < _abilities.Length; i++)
            {
                _abilities[i].Tick();
            }
        }

        private void OnDisable()
        {
            if (_abilities != null)
            {
                for (int i = 0; i < _abilities.Length; i++)
                {
                    _abilities[i].Dispose();
                }
            }

            _carInputProvider.UseNitroAction.performed -= Handle_UseNitroAction_Performed;
            _carInputProvider.UseNitroAction.canceled -= Handle_UseNitroAction_Canceled;
            _carInputProvider.UseShockwaveAction.performed -= Handle_UseShockwaveAction_Performed;
            _carInputProvider.UseShockwaveAction.canceled -= Handle_UseShockwaveAction_Canceled;
            _carInputProvider.UseSpeedbreakerAction.performed -= Handle_UseSpeedBreakerAction_Performed;
            _carInputProvider.UseSpeedbreakerAction.canceled -= Handle_UseSpeedBreakerAction_Canceled;
        }

        private void SetupAbilityInputs()
        {
            _carInputProvider.UseNitroAction.performed += Handle_UseNitroAction_Performed;
            _carInputProvider.UseNitroAction.canceled += Handle_UseNitroAction_Canceled;

            _carInputProvider.UseShockwaveAction.performed += Handle_UseShockwaveAction_Performed;
            _carInputProvider.UseShockwaveAction.canceled += Handle_UseShockwaveAction_Canceled;

            _carInputProvider.UseSpeedbreakerAction.performed += Handle_UseSpeedBreakerAction_Performed;
            _carInputProvider.UseSpeedbreakerAction.canceled += Handle_UseSpeedBreakerAction_Canceled;
        }

        private void SetupAbilities()
        {
            if (_abilityHolder == null)
            {
                LogX.LogError($"{nameof(_abilityHolder)} are not set!");

                return;
            }

            _abilities = _abilityHolder.GetComponentsInChildren<BaseAbility>();
            if (_abilities == null || _abilities.Length == 0)
            {
                LogX.LogError("Abilities are not found!");

                return;
            }

            CacheAbilities();

            for (int i = 0; i < _abilities.Length; i++)
            {
                _abilities[i].Init();
            }
        }

        // ? it might be unnecessary in the future
        private bool CacheAbilities()
        {
            _nitroAbility = GetAbility(AbilityType.Nitro);
            if (_nitroAbility == null)
            {
                LogX.LogError("Nitro ability is not found!");

                return false;
            }

            _shockwaveAbility = GetAbility(AbilityType.Shockwave);
            if (_shockwaveAbility == null)
            {
                LogX.LogError("Shockwave ability is not found!");

                return false;
            }
            _speedBreakerAbility = GetAbility(AbilityType.Speedbreaker);
            if (_speedBreakerAbility == null)
            {
                LogX.LogError("Speedbreaker ability is not found!");

                return false;
            }

            return true;
        }

        #region Subscribe/Unsubscribe Methods
        private void Handle_UseNitroAction_Canceled(InputAction.CallbackContext context)
        {
            _nitroAbility.OnButtonReleased();
        }

        private void Handle_UseNitroAction_Performed(InputAction.CallbackContext context)
        {
            _nitroAbility.OnButtonPressed();
        }

        private void Handle_UseShockwaveAction_Canceled(InputAction.CallbackContext context)
        {
            _shockwaveAbility.OnButtonReleased();
        }

        private void Handle_UseShockwaveAction_Performed(InputAction.CallbackContext context)
        {
            _shockwaveAbility.OnButtonPressed();
        }

        private void Handle_UseSpeedBreakerAction_Canceled(InputAction.CallbackContext context)
        {
            _speedBreakerAbility.OnButtonReleased();
        }

        private void Handle_UseSpeedBreakerAction_Performed(InputAction.CallbackContext context)
        {
            _speedBreakerAbility.OnButtonPressed();
        }
        #endregion
    }
}
