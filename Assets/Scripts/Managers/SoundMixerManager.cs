using System;
using Framework.Services;
using Game.Settings;
using Game.Utils;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Managers
{
    public class SoundMixerManager : MonoBehaviour, IGlobalService
    {
        public const string MasterVolumeParam = "MasterVolume";
        public const string MainMenuMusicVolumeParam = "MainMenuMusicVolume";
        public const string GameplayMusicVolumeParam = "GameplayMusicVolume";
        public const string UIVolumeParam = "UIVolume";
        public const string EffectsVolumeParam = "EffectsVolume";
        public const string EngineVolumeParam = "EngineVolume";

        [Header("References")]
        [SerializeField] private AudioMixer _audioMixer;

        public void SetFloat(string paramName, float value)
        {
            float decibelValue = SoundUtils.ConvertVolumeToDecibels(value);

            _audioMixer.SetFloat(paramName, decibelValue);
        }

        public void ResetParams()
        {
            GameSettings gameSettings = ServiceLocator
               .Global.GetService<GameSettingsManager>()
               .Settings;

            SetFloat(MasterVolumeParam, gameSettings.MasterVolume);
            SetFloat(MainMenuMusicVolumeParam, gameSettings.MainMenuMusicVolume);
            SetFloat(GameplayMusicVolumeParam, gameSettings.GameplayMusicVolume);
            SetFloat(UIVolumeParam, gameSettings.UIVolume);
            SetFloat(EffectsVolumeParam, gameSettings.EffectsVolume);
            SetFloat(EngineVolumeParam, gameSettings.EngineVolume);
        }

        public void TransitionToPause()
        {
            SetFloat(EngineVolumeParam, 0f);
            SetFloat(EffectsVolumeParam, 0f);
            SetFloat(GameplayMusicVolumeParam, 0f);
        }

        public void TransitionToGameplay()
        {
            var gameSettings = ServiceLocator.Global.GetService<GameSettingsManager>().Settings;

            SetFloat(EngineVolumeParam, gameSettings.EngineVolume);
            SetFloat(EffectsVolumeParam, gameSettings.EffectsVolume);
            SetFloat(GameplayMusicVolumeParam, gameSettings.GameplayMusicVolume);
        }

        public void AfterInit()
        {
            // Apply saved settings to the mixer
            GameSettings gameSettings = ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings;

            SetFloat(MasterVolumeParam, gameSettings.MasterVolume);
            SetFloat(MainMenuMusicVolumeParam, gameSettings.MainMenuMusicVolume);
            SetFloat(GameplayMusicVolumeParam, gameSettings.GameplayMusicVolume);
            SetFloat(UIVolumeParam, gameSettings.UIVolume);
            SetFloat(EffectsVolumeParam, gameSettings.EffectsVolume);
            SetFloat(EngineVolumeParam, gameSettings.EngineVolume);
        }

        public void Dispose()
        {
            // noop
        }

        public void Init()
        {
            // noop
        }
    }
}