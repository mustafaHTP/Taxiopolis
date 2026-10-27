using Framework.Services;
using Game.Managers;
using Game.Settings;
using Game.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Screens.Options
{
    public class OptionAudioPanel : UIPanel
    {
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _mainMenuMusicSlider;
        [SerializeField] private Slider _gameplayMusicSlider;
        [SerializeField] private Slider _uiSlider;
        [SerializeField] private Slider _effectsSlider;
        [SerializeField] private Slider _engineSlider;

        public override void Show()
        {
            GameSettings gameSettings = ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings;

            _masterSlider.value = gameSettings.MasterVolume;
            _mainMenuMusicSlider.value = gameSettings.MainMenuMusicVolume;
            _gameplayMusicSlider.value = gameSettings.GameplayMusicVolume;
            _uiSlider.value = gameSettings.UIVolume;
            _effectsSlider.value = gameSettings.EffectsVolume;
            _engineSlider.value = gameSettings.EngineVolume;

            base.Show();
        }

        public void OnMasterVolumeChanged()
        {
            ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.MasterVolume = _masterSlider.value;
        }

        public void OnMainMenuMusicVolumeChanged()
        {
            ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.MainMenuMusicVolume = _mainMenuMusicSlider.value;
        }

        public void OnGameplayMusicVolumeChanged()
        {
            ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.GameplayMusicVolume = _gameplayMusicSlider.value;
        }

        public void OnUIVolumeChanged()
        {
            ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.UIVolume = _uiSlider.value;
        }

        public void OnEffectsVolumeChanged()
        {
            ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.EffectsVolume = _effectsSlider.value;
        }

        public void OnEngineVolumeChanged()
        {
            ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.EngineVolume = _engineSlider.value;
        }
    }
}
