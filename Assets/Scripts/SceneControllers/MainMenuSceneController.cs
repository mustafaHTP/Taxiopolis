using Framework.Services;
using Game.Managers;
using Game.SceneServiceInstallers;
using Game.Systems.PedestrianSystem;
using Game.Systems.SoundSystem;
using Game.Systems.TrafficSystem.Imposter;
using Game.UI;
using Game.UI.Screens.MainMenu;
using Game.Utils;
using UnityEngine;

namespace Game.SceneControllers
{
    public class MainMenuSceneController : BaseSceneController
    {
        [Header("References")]
        [SerializeField] private ImposterTrafficManager _imposterTrafficManager;
        [SerializeField] private ImposterPedestrianManager _imposterPedestrianManager;
        [SerializeField] private SoundData _mainMenuMusicSound;

        public override void Init()
        {
            if (_imposterTrafficManager == null)
            {
                LogX.LogError("Imposter Traffic Manager is null", this);

                return;
            }

            if (_imposterPedestrianManager == null)
            {
                LogX.LogError("Imposter Pedestrian Manager is null", this);
            }
        }

        public override void StartScene()
        {
            ServiceLocator.Global.GetService<InputManager>().DisableAllInput();
            ServiceLocator.Global.GetService<InputManager>().EnableUIInput();

            ServiceLocator.Scene.GetService<SceneUIInstaller>().Install();

            ServiceLocator
                .Global.GetService<UIManager>()
                .PushUI(ServiceLocator.Scene.GetService<UIMainMenuScreen>());

            ServiceLocator
                .Global.GetService<MusicManager>()
                .AddSoundToPlayList(_mainMenuMusicSound);
            ServiceLocator
                .Global.GetService<MusicManager>()
                .StartPlaylist();

            _imposterTrafficManager.StartTraffic();
            _imposterPedestrianManager.StartTraffic();
        }
    }
}
