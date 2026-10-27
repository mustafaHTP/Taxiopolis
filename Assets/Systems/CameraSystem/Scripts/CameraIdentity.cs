using Framework.Services;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Systems.CameraSystem
{
    /// <summary>
    /// Tags a CinemachineCamera and announces its presence to CameraManager.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraIdentity : MonoBehaviour
    {
        [SerializeField] private CameraRole _role;
        [Tooltip("Position in the switch cycle, lowest first. Ties break by registration order.")]
        [SerializeField] private int _order;

        public CameraRole Role => _role;
        public CinemachineCamera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = GetComponent<CinemachineCamera>();
                }
                return _camera;
            }
            private set
            {
                _camera = value;
            }
        }
        public int Order => _order;

        private CinemachineCamera _camera;
    }
}