using Framework.Services;
using Game.Input;
using Game.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Systems.CameraSystem
{
    /// <summary>
    /// Translates the "switch camera" input into an intent CameraManager understands.
    /// Holds no camera references and no priority logic - it doesn't know or care how
    /// many player cameras exist.
    /// </summary>
    public class PlayerCameraSwitcher : MonoBehaviour
    {
        private InputAction _switchCameraAction;
        private CameraManager _cameraManager;

        private void Awake()
        {
            _cameraManager = ServiceLocator.Global.GetService<CameraManager>();
        }

        private void Start()
        {
            if (!TryGetComponent(out PlayerInputProvider playerInputProvider))
            {
                LogX.LogError($"{nameof(PlayerCameraSwitcher)} did not find {nameof(PlayerInputProvider)}", this);

                return;
            }

            playerInputProvider.SwitchCameraAction.performed += PlayerInputProvider_SwitchCamera_Performed;
        }

        private void OnDisable()
        {
            if (_switchCameraAction != null)
            {
                _switchCameraAction.performed -= PlayerInputProvider_SwitchCamera_Performed;
            }
        }

        private void PlayerInputProvider_SwitchCamera_Performed(InputAction.CallbackContext _)
        {
            _cameraManager.SwitchToNext();
        }
    }
}