using Framework.Services;
using Game.Managers;
using Game.UI.Screens.MainMenu;
using Game.UI.Screens.Options;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.SceneServiceInstallers
{
    public class MainMenuSceneServiceInstaller : SceneServiceInstaller
    {
        [SerializeField] private Volume _volume;
        [SerializeField] private SceneUIInstaller _sceneUIInstaller;

        protected override void RegisterDependencies()
        {
            base.RegisterDependencies();

            ServiceLocator.Scene.RegisterService(_volume);
            ServiceLocator.Scene.RegisterService(_sceneUIInstaller);
        }
    }
}