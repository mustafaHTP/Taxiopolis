using Coffee.UIEffects;
using Game.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Screens.Loading
{
    public class UILoadingScreen : UIScreen
    {
        // no need to register in scene, because it is under global ui manager
        public override void RegisterInScene()
        {
            // noop
        }
    }
}
