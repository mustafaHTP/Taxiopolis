using Framework.Services;
using Game.Settings;
using Game.UI.Common;
using Game.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Screens.Options
{
    public class UIOptionsScreen : UIScreen
    {
        [Header("Buttons")]
        [SerializeField] private Button _resetSettingsButton;
        [SerializeField] private Button _closePanelButton;

        [Header("Option Panels")]
        [SerializeField] private OptionAudioPanel _optionAudioPanel;
        [SerializeField] private OptionGraphicsPanel _optionGraphicsPanel;
        [SerializeField] private OptionControlsPanel _optionControlsPanel;

        private UIPanel _currentPanel;

        public override void Init()
        {
            _resetSettingsButton.onClick.AddListener(() =>
            {
                ServiceLocator.Global.GetService<GameSettingsManager>().Settings.ResetSettings();
            });

            _closePanelButton.onClick.AddListener(() =>
            {
                ServiceLocator.Global.GetService<UIManager>().PopUI();
            });
        }

        public override void Dispose()
        {
            _resetSettingsButton.onClick.RemoveAllListeners();
            _closePanelButton.onClick.RemoveAllListeners();
        }

        public override void Show()
        {
            HideAllPanels();

            _optionAudioPanel.Show();

            _currentPanel = _optionAudioPanel;

            ActivateObject();
        }

        public override void Hide()
        {
            HideAllPanels();

            _currentPanel = null;

            DeactivateObject();
        }

        public void ShowPanel(UIPanel panel)
        {
            if (_currentPanel == panel) return;

            HideAllPanels();

            panel.Show();

            _currentPanel = panel;
        }

        public void HideAllPanels()
        {
            _optionAudioPanel.Hide();
            _optionGraphicsPanel.Hide();
            _optionControlsPanel.Hide();
        }
    }
}