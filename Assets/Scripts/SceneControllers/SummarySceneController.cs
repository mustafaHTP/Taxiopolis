using Framework.Services;
using Game.Managers;
using Game.SceneServiceInstallers;
using Game.UI;
using Game.UI.Screens.Summary;
using UnityEngine;

namespace Game.SceneControllers
{
    public class SummarySceneController : BaseSceneController
    {
        public override void StartScene()
        {
            ServiceLocator.Scene.GetService<SceneUIInstaller>().Install();
            ServiceLocator
                .Global.GetService<UIManager>()
                .PushUI(ServiceLocator.Scene.GetService<UISummaryScreen>());
            ServiceLocator.Global.GetService<InputManager>().EnableUIInput();
        }
    }
}
