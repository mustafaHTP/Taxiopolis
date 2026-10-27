using Unity.VisualScripting;
using UnityEngine;

namespace Game.SceneControllers
{
    public class BaseSceneController : MonoBehaviour
    {
        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            StartScene();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public virtual void Init() { }
        public virtual void Dispose() { }
        public virtual void StartScene() { }
    }
}
