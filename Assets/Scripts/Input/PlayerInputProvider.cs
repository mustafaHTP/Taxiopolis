using Framework.Services;
using Game.Managers;
using Game.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    public class PlayerInputProvider : MonoBehaviour, ICarInputProvider
    {
        private GameInput _gameInput;

        public FrameInput FrameInput { get; private set; }

        #region Input Actions
        /// <summary>
        /// Ground Controls
        /// </summary>
        public InputAction GasAction { get; private set; }
        public InputAction BrakeAction { get; private set; }
        public InputAction SteerAction { get; private set; }
        public InputAction HandbrakeAction { get; private set; }
        public InputAction SwitchCameraAction { get; private set; }
        public InputAction ResetCarRotation { get; private set; }
        public InputAction HornAction { get; private set; }
        public InputAction UseNitroAction { get; private set; }
        public InputAction UseShockwaveAction { get; private set; }
        public InputAction UseSpeedbreakerAction { get; private set; }
        public InputAction RecoverAction { get; private set; }
        #endregion

        private void Awake()
        {
            _gameInput = ServiceLocator.Global.GetService<InputManager>().GameInput;

            GasAction = _gameInput.Car.Gas;
            BrakeAction = _gameInput.Car.Brake;
            SteerAction = _gameInput.Car.Steer;
            HandbrakeAction = _gameInput.Car.Handbrake;
            SwitchCameraAction = _gameInput.Game.SwitchCamera;
            HornAction = _gameInput.Car.Horn;

            UseNitroAction = _gameInput.Car.UseNitro;
            UseShockwaveAction = _gameInput.Car.UseShockwave;
            UseSpeedbreakerAction = _gameInput.Car.UseSpeedbreaker;

            RecoverAction = _gameInput.Car.Recover;
        }

        private void Update()
        {
            FrameInput = new(
                GasAction.ReadValue<float>(),
                BrakeAction.ReadValue<float>(),
                SteerAction.ReadValue<float>(),
                HandbrakeAction.ReadValue<float>());
        }

        public InputAction FindAction(string actionMap, string actionName)
        {
            if (_gameInput == null)
            {
                LogX.LogError("GameInput is null. Cannot find action.");

                return null;
            }

            var foundActionMap = _gameInput.asset.FindActionMap(actionMap);
            if (foundActionMap == null)
            {
                LogX.LogError($"Action Map not found: {actionMap}");
            }

            var action = foundActionMap.FindAction(actionName);
            if (action == null)
            {
                LogX.LogError($"Action not found: {actionName} in Action Map: {actionMap}");
            }

            return action;
        }
    }
}
