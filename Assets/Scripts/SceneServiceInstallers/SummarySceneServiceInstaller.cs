using Framework.Services;
using Game.UI.Screens.Summary;
using UnityEngine;

namespace Game.SceneServiceInstallers
{
    public class SummarySceneServiceInstaller : SceneServiceInstaller
    {
        [SerializeField] private SceneUIInstaller _sceneUIInstaller;

        protected override void RegisterDependencies()
        {
            base.RegisterDependencies();

            ServiceLocator.Scene.RegisterService(_sceneUIInstaller);
        }
    }
}
