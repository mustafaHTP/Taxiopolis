using Framework.Services;
using Game.Managers;
using Game.UI.Common;
using Game.UI.Screens.Options;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Screens.MainMenu
{
    public class UIMainMenuScreen : UIScreen
    {
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _optionsButton;
        [SerializeField] private Button _quitButton;

        [Header("Dependencies")]
        [SerializeField] private UIOptionsScreen _optionsScreen;

        public override void Init()
        {
            _playButton
                .onClick
                .AddListener(() => ServiceLocator.Global.GetService<GameManager>().LoadGameSceneAsync());
            _optionsButton
                .onClick
                .AddListener(() => ServiceLocator.Global.GetService<UIManager>().PushUI(_optionsScreen));
            _quitButton
                .onClick
                .AddListener(() => ServiceLocator.Global.GetService<GameManager>().QuitGame());
        }
    }
}
