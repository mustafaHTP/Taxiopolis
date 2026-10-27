using Game.Input;
using Game.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarSfxHandler : MonoBehaviour
    {
        [Header("Engine Sfx")]
        [SerializeField] private AudioSource _engineAudioSource;
        [SerializeField] private float _minEnginePitch = 0.8f;
        [SerializeField] private float _maxEnginePitch = 2.0f;

        [Header("Horn Sfx")]
        [SerializeField] private AudioSource _hornAudioSource;

        [Header("Nitro Sfx")]
        [SerializeField] private AudioSource _nitroStartAudioSource;
        [SerializeField] private AudioSource _nitroLoopAudioSource;
        [SerializeField] private AudioSource _nitroEndAudioSource;

        [Header("Shockwave Sfx")]
        [SerializeField] private AudioSource _shockwaveAudioSource;

        [Header("Speedbreaker Sfx")]
        [SerializeField] private AudioSource _speedbreakerStartAudioSource;
        [SerializeField] private AudioSource _speedbreakerLoopAudioSource;
        [SerializeField] private AudioSource _speedbreakerEndAudioSource;

        [Header("Drift Sfx")]
        [SerializeField] private AudioSource _driftAudioSource;

        private CarController _carController;
        private ICarInputProvider _carInputProvider;

        private bool _isEnginePlaying = false;

        public void PlayDriftSfx()
        {
            _driftAudioSource.Play();
        }

        public void StopDriftSfx()
        {
            _driftAudioSource.Stop();
        }

        public void PlaySpeedbreakerStart()
        {
            _speedbreakerStartAudioSource.Play();
        }

        public void PlaySpeedbreakerLoop()
        {
            _speedbreakerLoopAudioSource.Play();
        }

        public void StopSpeedbreakerLoop()
        {
            _speedbreakerLoopAudioSource.Stop();
        }

        public void PlaySpeedbreakerEnd()
        {
            _speedbreakerEndAudioSource.Play();
        }

        public void PlayEngine()
        {
            // Change pitch based on normalized speed (0 to 1)
            _engineAudioSource.Play();
            _isEnginePlaying = true;
        }

        public void PlayHorn()
        {
            _hornAudioSource.Play();
        }

        public void PlayShockwaveSfx()
        {
            _shockwaveAudioSource.Play();
        }

        public void PlayNitroStartSfx()
        {
            _nitroStartAudioSource.Play();
        }

        public void PlayNitroLoopSfx()
        {
            _nitroLoopAudioSource.Play();
        }

        public void StopNitroLoopSfx()
        {
            _nitroLoopAudioSource.Stop();
        }

        public void PlayNitroStopSfx()
        {
            _nitroEndAudioSource.Play();
        }

        public void PlayNitroEndSfx()
        {
            _nitroEndAudioSource.Play();
        }

        public void StopNitroEndSfx()
        {
            _nitroEndAudioSource.Stop();
        }

        private void Awake()
        {
            if (!TryGetComponent(out _carController))
            {
                LogX.LogError("CarController component not found on CarSfxHandler GameObject.", this);

                return;
            }
        }

        private void OnEnable()
        {
            _carController.OnEngineStarted += CarController_OnEngineStarted;
            _carController.OnDriftStarted += CarController_OnDriftStarted;
            _carController.OnDriftEnded += CarController_OnDriftEnded;
        }

        private void Start()
        {
            if (!TryGetComponent<ICarInputProvider>(out _carInputProvider))
            {
                LogX.LogError("ICarInputProvider component not found on CarSfxHandler GameObject.", this);

                return;
            }

            _carInputProvider.HornAction.performed += CarInputProvider_HornAction;
        }

        private void Update()
        {
            if (_isEnginePlaying)
            {
                float speedNormalized = _carController.CarState.SpeedNormalized;
                float targetPitch = Mathf.Lerp(_minEnginePitch, _maxEnginePitch, speedNormalized);
                _engineAudioSource.pitch = targetPitch;
            }
        }

        private void OnDisable()
        {
            _carController.OnEngineStarted -= CarController_OnEngineStarted;
            _carController.OnDriftStarted -= CarController_OnDriftStarted;
            _carController.OnDriftEnded -= CarController_OnDriftEnded;
            _carInputProvider.HornAction.performed -= CarInputProvider_HornAction;
        }

        private void CarController_OnEngineStarted()
        {
            PlayEngine();
        }

        private void CarController_OnDriftEnded()
        {
            StopDriftSfx();
        }

        private void CarController_OnDriftStarted()
        {
            PlayDriftSfx();
        }

        private void CarInputProvider_HornAction(InputAction.CallbackContext _)
        {
            PlayHorn();
        }
    }
}
