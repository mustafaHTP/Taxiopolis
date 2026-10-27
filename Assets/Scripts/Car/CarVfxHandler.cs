using Framework.Services;
using Game.Car;
using Game.Managers;
using Game.Utils;
using UnityEngine;

namespace Game.VFX
{
    public class CarVfxHandler : MonoBehaviour
    {
        [Header("Car Smoke VFX")]
        [SerializeField] private ParticleSystem[] _carSmokes;

        private CarController _carController;

        public void PlayCarSmoke()
        {
            for (int i = 0; i < _carSmokes.Length; i++)
            {
                ParticleSystem.EmissionModule emissionModule = _carSmokes[i].emission;
                emissionModule.enabled = true;
            }
        }

        public void StopCarSmoke()
        {
            for (int i = 0; i < _carSmokes.Length; i++)
            {
                ParticleSystem.EmissionModule emissionModule = _carSmokes[i].emission;
                emissionModule.enabled = false;
            }
        }

        private void Awake()
        {
            _carController = GetComponent<CarController>();
        }

        private void Start()
        {
            StopCarSmoke();
        }

        private void OnEnable()
        {
            _carController.OnDriftStarted += PlayCarSmoke;
            _carController.OnDriftEnded += StopCarSmoke;
        }

        private void OnDisable()
        {
            _carController.OnDriftStarted -= PlayCarSmoke;
            _carController.OnDriftEnded -= StopCarSmoke;
        }
    }
}
