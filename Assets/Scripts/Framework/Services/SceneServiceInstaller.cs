using UnityEngine;

namespace Framework.Services
{
    [DefaultExecutionOrder(-2)]
    /// <summary>
    /// Must be run before default Awake execution time
    /// so that every object in scene can use scene services in Awake
    /// </summary>
    public abstract class SceneServiceInstaller : MonoBehaviour
    {
        // no need to clear dependency since i already making it global manager
        protected virtual void RegisterDependencies() { }

        private void Awake()
        {
            RegisterDependencies();
        }
    }
}
