using Framework.Services;
using Game.Managers;
using Game.UI.Common;
using Game.UI.Screens.Options;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Screens.PauseMenu
{
    public class UIPauseMenuScreen : UIScreen
    {
        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _optionsMenuButton;

        [Header("Dependencies")]
        [SerializeField] private UIOptionsScreen _optionsScreen;

        public override void Init()
        {
            _resumeButton
                .onClick
                .AddListener(() => ServiceLocator.Global.GetService<GameManager>().ResumeGame());
            _mainMenuButton
                .onClick
                .AddListener(() => ServiceLocator.Global.GetService<GameManager>().LoadMainMenuSceneAsync());
            _optionsMenuButton
                .onClick
                .AddListener(() => ServiceLocator.Global.GetService<UIManager>().PushUI(_optionsScreen));
        }

        public override void Dispose()
        {
            _resumeButton.onClick.RemoveAllListeners();
            _mainMenuButton.onClick.RemoveAllListeners();
            _optionsMenuButton.onClick.RemoveAllListeners();
        }
    }
}
