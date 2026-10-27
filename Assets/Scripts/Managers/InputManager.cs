using Framework.Services;
using Game.Input;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Game.Managers
{
    public class InputManager : MonoBehaviour, IGlobalService
    {
        public event Action<CustomDeviceType, bool> OnDeviceChanged;

        private bool _isInitialized = false;
        private GameInput _gameInput;
        private InputDevice _currentDevice;

        public GameInput GameInput => _gameInput;
        public CustomDeviceType CurrentCustomDeviceType { get; private set; }

        public void Init()
        {
            if (_isInitialized) return;

            _gameInput = new();
            InputSystem.onEvent += InputSystem_OnInput;

            _isInitialized = true;
        }

        public void DisableAllInput()
        {
            _gameInput.UI.Disable();
            _gameInput.Game.Disable();
            _gameInput.Car.Disable();
        }

        public void EnableUIInput()
        {
            _gameInput.UI.Enable();
        }

        public void EnableCarInput()
        {
            _gameInput.Car.Enable();
        }

        public void EnableGameInput()
        {
            _gameInput.Game.Enable();
        }

        public void Dispose()
        {
            InputSystem.onEvent -= InputSystem_OnInput;
        }

        public void AfterInit()
        {
            // noop
        }

        private void InputSystem_OnInput(InputEventPtr eventPtr, InputDevice device)
        {
            if (device == null) return;
            if (_currentDevice == device) return;

            if (device is Mouse)
            {
                _currentDevice = device;
                CurrentCustomDeviceType = CustomDeviceType.KeyboardAndMouse;
                OnDeviceChanged?.Invoke(CurrentCustomDeviceType, true);

                Cursor.visible = true;
                //Cursor.lockState = CursorLockMode.None;
            }
            else if (device is Keyboard)
            {
                _currentDevice = device;
                CurrentCustomDeviceType = CustomDeviceType.KeyboardAndMouse;
                OnDeviceChanged?.Invoke(CurrentCustomDeviceType, false);

                Cursor.visible = false;
                //Cursor.lockState = CursorLockMode.Locked;
            }
            else if (device is Gamepad)
            {
                _currentDevice = device;
                CurrentCustomDeviceType = CustomDeviceType.Gamepad;
                OnDeviceChanged?.Invoke(CurrentCustomDeviceType, false);

                Cursor.visible = false;
                //Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}