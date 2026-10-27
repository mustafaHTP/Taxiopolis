using Framework.Services;
using UnityEngine;

namespace Game.UI.Common
{
    // covers whole screen, container for ui panels
    public abstract class UIScreen : BaseUIElement
    {
        public virtual void RegisterInScene()
        {
            ServiceLocator.Scene.RegisterService(GetType(), this);
        }
    }
}
