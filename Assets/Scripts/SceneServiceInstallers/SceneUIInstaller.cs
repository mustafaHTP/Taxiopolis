using Game.UI.Common;
using Game.Utils;
using UnityEngine;

namespace Game.SceneServiceInstallers
{
    /// <summary>
    /// Explicitly registers and initializes every <see cref="UIScreen"/> under a UI root,
    /// instead of letting each screen register itself in Awake/Start.
    ///
    /// Why this exists: UI screens usually depend on scene services (resolved through the
    /// scene service locator). If each screen initialized itself, Unity's arbitrary Awake
    /// order could make a screen resolve a service before it is registered, causing
    /// null reference errors. By driving initialization from here, the scene installer
    /// controls the exact order: services first, then UI.
    /// </summary>
    public class SceneUIInstaller : MonoBehaviour
    {
        [SerializeField] private Transform _uiRoot;

        private UIScreen[] _screens;

        // call from scene installer, after scene services are registered
        public void Install()
        {
            if (_uiRoot == null)
            {
                LogX.LogError("UI root is not set in scene ui installer");

                return;
            }

            _screens = _uiRoot.GetComponentsInChildren<UIScreen>(true);

            foreach (var screen in _screens) screen.RegisterInScene();
            foreach (var screen in _screens) { screen.Init(); screen.HideInstant(); }
        }
    }
}
