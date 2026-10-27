using System;
using Framework.Services;
using Game.Systems.SoundSystem;
using UnityEngine;

namespace Game.Car
{
    public class CarWaterDetector : MonoBehaviour
    {
        [SerializeField] private SoundData _waterSplashSoundData;

        public event Action OnWaterDetected;

        [SerializeField] private WaterTrigger _waterTrigger;

        private void OnEnable()
        {
            _waterTrigger.OnWaterDetected += WaterTrigger_OnWaterDetected;
        }

        private void WaterTrigger_OnWaterDetected()
        {
            ServiceLocator
                .Global
                .GetService<SoundManager>()
                .PlaySound(_waterSplashSoundData);
            OnWaterDetected?.Invoke();
        }

        private void OnDisable()
        {
            _waterTrigger.OnWaterDetected -= WaterTrigger_OnWaterDetected;
        }
    }
}
