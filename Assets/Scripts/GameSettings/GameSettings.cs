using Framework.Services;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Managers;
using Game.Misc;

namespace Game.Settings
{
    public class GameSettings
    {
        // Audio Keys
        public const string MasterVolumeKey = "MasterVolume";
        public const string MainMenuMusicVolumeKey = "MainMenuMusicVolume";
        public const string GameplayMusicVolumeKey = "GameplayMusicVolume";
        public const string UIVolumeKey = "UIVolume";
        public const string EffectsVolumeKey = "EffectsVolume";
        public const string EngineVolumeKey = "EngineVolume";

        // Graphics Keys
        public const string ResolutionXKey = "ResolutionX";
        public const string ResolutionYKey = "ResolutionY";
        public const string IsVsyncOnKey = "IsVsyncOn";

        // Gameplay Keys
        public const string ToggleControlHintsKey = "ToggleControlHints";

        // Defaults
        public const float DefaultMasterVolume = 0.8f;
        public const float DefaultMainMenuMusicVolume = 0.8f;
        public const float DefaultGameplayMusicVolume = 0.8f;
        public const float DefaultUIVolume = 0.8f;
        public const float DefaultEffectsVolume = 0.8f;
        public const float DefaultEngineVolume = 0.8f;
        public static readonly Vector2Int DefaultResolution = new(1920, 1080);
        public const bool DefaultIsVsyncOn = true;
        public const bool DefaultToggleControlHints = true;

        private readonly ScreenResolutionContainer _screenResolutionContainer;
        private readonly List<Vector2Int> _availableResolutions = new();

        public GameSettings(ScreenResolutionContainer screenResolutionContainer)
        {
            _screenResolutionContainer = screenResolutionContainer;
            ConstructAvailableResolutions();
        }

        private void ConstructAvailableResolutions()
        {
            for (int i = 0; i < _screenResolutionContainer.ScreenResolutions.Length; i++)
            {
                _availableResolutions.Add(_screenResolutionContainer.ScreenResolutions[i]);
            }
        }

        public bool ToggleControlHints
        {
            get
            {
                int isControlHintsToggled = PlayerPrefs.GetInt(ToggleControlHintsKey, DefaultToggleControlHints ? 1 : 0);

                switch (isControlHintsToggled)
                {
                    case 1:
                        return true;
                    case 0:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException($"{nameof(ToggleControlHints)} must be 1 or 0 !!");
                }
            }
            set
            {
                PlayerPrefs.SetInt(ToggleControlHintsKey, value ? 1 : 0);
            }
        }

        public float MasterVolume
        {
            get
            {
                return PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
            }
            set
            {
                PlayerPrefs.SetFloat(MasterVolumeKey, value);
                ServiceLocator.Global.GetService<SoundMixerManager>().SetFloat(SoundMixerManager.MasterVolumeParam, value);
            }
        }
        public float MainMenuMusicVolume
        {
            get
            {
                return PlayerPrefs.GetFloat(MainMenuMusicVolumeKey, DefaultMainMenuMusicVolume);
            }
            set
            {
                PlayerPrefs.SetFloat(MainMenuMusicVolumeKey, value);
                ServiceLocator.Global.GetService<SoundMixerManager>().SetFloat(SoundMixerManager.MainMenuMusicVolumeParam, value);
            }
        }
        public float GameplayMusicVolume
        {
            get
            {
                return PlayerPrefs.GetFloat(GameplayMusicVolumeKey, DefaultGameplayMusicVolume);
            }
            set
            {
                PlayerPrefs.SetFloat(GameplayMusicVolumeKey, value);
                ServiceLocator.Global.GetService<SoundMixerManager>().SetFloat(SoundMixerManager.GameplayMusicVolumeParam, value);
            }
        }
        public float EffectsVolume
        {
            get
            {
                return PlayerPrefs.GetFloat(EffectsVolumeKey, DefaultEffectsVolume);
            }
            set
            {
                PlayerPrefs.SetFloat(EffectsVolumeKey, value);
                ServiceLocator.Global.GetService<SoundMixerManager>().SetFloat(SoundMixerManager.EffectsVolumeParam, value);
            }
        }
        public float EngineVolume
        {
            get
            {
                return PlayerPrefs.GetFloat(EngineVolumeKey, DefaultEngineVolume);
            }
            set
            {
                PlayerPrefs.SetFloat(EngineVolumeKey, value);
                ServiceLocator.Global.GetService<SoundMixerManager>().SetFloat(SoundMixerManager.EngineVolumeParam, value);
            }
        }
        public float UIVolume
        {
            get
            {
                return PlayerPrefs.GetFloat(UIVolumeKey, DefaultUIVolume);
            }
            set
            {
                PlayerPrefs.SetFloat(UIVolumeKey, value);
                ServiceLocator.Global.GetService<SoundMixerManager>().SetFloat(SoundMixerManager.UIVolumeParam, value);
            }
        }
        public Vector2Int Resolution
        {
            get
            {
                int resX = PlayerPrefs.GetInt(ResolutionXKey, DefaultResolution.x);
                int resY = PlayerPrefs.GetInt(ResolutionYKey, DefaultResolution.y);

                return new Vector2Int(resX, resY);
            }
            set
            {
                int resX = value.x;
                int resY = value.y;

                PlayerPrefs.SetInt(ResolutionXKey, resX);
                PlayerPrefs.SetInt(ResolutionYKey, resY);

                // Apply the new resolution immediately
                //Screen.SetResolution(resX, resy, FullScreenMode.ExclusiveFullScreen);
                ApplyResolution(resX, resY);
            }
        }
        public bool IsVsyncOn
        {
            get
            {
                int isVsyncOn = PlayerPrefs.GetInt(IsVsyncOnKey, DefaultIsVsyncOn ? 1 : 0);
                switch (isVsyncOn)
                {
                    case 1: return true;
                    case 0: return false;
                    default: throw new ArgumentException("Parameter should be either 1 or 0 !");
                }
            }
            set
            {
                int isVsyncOn = value ? 1 : 0;
                PlayerPrefs.SetInt(IsVsyncOnKey, isVsyncOn);

                QualitySettings.vSyncCount = value ? 1 : 0;

                if (value)
                {
                    Application.targetFrameRate = -1; // Let VSync control the frame rate
                }
            }
        }

        public List<Vector2Int> AvailableResolutions => _availableResolutions;

        /// <summary>
        /// Applies all settings to the runtime systems: audio mixer parameters, screen resolution and VSync.
        /// Reads current values (which come from PlayerPrefs via the properties) and applies them.
        /// </summary>
        public void ApplyAllSettings()
        {
            // Apply audio settings
            var mixer = ServiceLocator.Global.GetService<SoundMixerManager>();
            if (mixer != null)
            {
                mixer.SetFloat(SoundMixerManager.MasterVolumeParam, MasterVolume);
                mixer.SetFloat(SoundMixerManager.MainMenuMusicVolumeParam, MainMenuMusicVolume);
                mixer.SetFloat(SoundMixerManager.GameplayMusicVolumeParam, GameplayMusicVolume);
                mixer.SetFloat(SoundMixerManager.UIVolumeParam, UIVolume);
                mixer.SetFloat(SoundMixerManager.EffectsVolumeParam, EffectsVolume);
                mixer.SetFloat(SoundMixerManager.EngineVolumeParam, EngineVolume);
            }

            // Apply resolution
            // var res = Resolution;
            // Screen.SetResolution(res.x, res.y, Screen.fullScreen);
            ApplyResolution(Resolution.x, Resolution.y);

            // Apply VSync
            QualitySettings.vSyncCount = IsVsyncOn ? 1 : 0;
            if (IsVsyncOn)
            {
                Application.targetFrameRate = -1; // Let VSync control the frame rate
            }
        }

        public void ResetSettings()
        {
            // Audio
            PlayerPrefs.DeleteKey(MasterVolumeKey);
            PlayerPrefs.DeleteKey(MainMenuMusicVolumeKey);
            PlayerPrefs.DeleteKey(GameplayMusicVolumeKey);
            PlayerPrefs.DeleteKey(UIVolumeKey);
            PlayerPrefs.DeleteKey(EffectsVolumeKey);
            PlayerPrefs.DeleteKey(EngineVolumeKey);

            // Graphics
            PlayerPrefs.DeleteKey(ResolutionXKey);
            PlayerPrefs.DeleteKey(ResolutionYKey);
            PlayerPrefs.DeleteKey(IsVsyncOnKey);

            // Gameplay
            PlayerPrefs.DeleteKey(ToggleControlHintsKey);

            PlayerPrefs.Save();

            ApplyAllSettings();
        }

        private void ApplyResolution(int resX, int resY)
        {
            Screen.SetResolution(resX, resY, FullScreenMode.FullScreenWindow);
        }
    }
}
