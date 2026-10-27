using System.Collections.Generic;
using Framework.Services;
using Game.Managers;
using Game.Utils;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Systems.CameraSystem
{
    /// <summary>
    /// Single source of truth for which camera is active. Cameras register themselves
    /// via CameraIdentity; nobody outside this class should ever touch a
    /// CinemachineCamera's Priority directly.
    /// </summary>
    public class CameraManager : MonoBehaviour, IGlobalService
    {
        public const int InactiveCameraPriority = 0;
        public const int ActiveCameraPriority = 10;

        // Kept sorted by CameraIdentity.Order at all times (see Register). Ties break
        // by registration order since insertion only moves past strictly-greater Order
        // values.
        private readonly List<CameraIdentity> _registeredCameras = new();

        private CinemachineCamera _activeCamera;

        public void Init()
        {
            // noop
        }

        public void AfterInit()
        {
            // noop
        }

        public void Dispose()
        {
            // noop
        }

        public void RegisterCameras()
        {
            CameraIdentity[] cameraIdentities = FindObjectsByType<CameraIdentity>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (cameraIdentities == null) return;

            for (int i = 0; i < cameraIdentities.Length; i++)
            {
                Register(cameraIdentities[i]);
            }
        }

        public void ClearCameras()
        {
            _registeredCameras.Clear();
        }

        /// <summary>
        /// Activates the first camera in registered cams. 
        /// Since cameras in sorted order, it activates the first camera.
        /// Useful for non-special scene setup
        /// </summary>
        public void ActivateHighestPriorityCamera()
        {
            if (_registeredCameras.Count == 0) return;

            ActivateCamera(_registeredCameras[0]);
        }

        /// <summary>
        /// Finds camera by name in registered cameras 
        /// </summary>
        /// <param name="cameraName"></param>
        /// <returns></returns> <summary>
        /// 
        /// </summary>
        /// <param name="cameraName"></param>
        /// <returns></returns>
        public CameraIdentity FindCameraByName(string cameraName)
        {
            for (int i = 0; i < _registeredCameras.Count; i++)
            {
                if (cameraName == _registeredCameras[i].name)
                {
                    return _registeredCameras[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Called by CameraIdentity.OnEnable. Inserted at the position matching its
        /// Order, so the switch cycle follows what you set in the inspector rather
        /// than whatever order OnEnable happened to fire in. If this is the very first
        /// camera to register, it becomes active automatically.
        /// </summary>
        private void Register(CameraIdentity identity)
        {
            if (!Application.isEditor && identity.Role == CameraRole.Debug)
            {
                return;
            }

            if (_registeredCameras.Contains(identity))
            {
                return;
            }

            int insertIndex = _registeredCameras.Count;

            for (int i = 0; i < _registeredCameras.Count; i++)
            {
                if (_registeredCameras[i].Order > identity.Order)
                {
                    insertIndex = i;
                    break;
                }
            }

            _registeredCameras.Insert(insertIndex, identity);
            identity.Camera.Priority = InactiveCameraPriority;

            if (_activeCamera == null)
            {
                ActivateCamera(identity);
            }
        }

        /// <summary>
        /// Called by CameraIdentity.OnDisable. Falls back to another registered camera
        /// if the one being removed was active, so a destroyed/disabled camera never
        /// leaves the scene with nothing active.
        /// </summary>
        private void Unregister(CameraIdentity identity)
        {
            int index = _registeredCameras.IndexOf(identity);

            if (index < 0)
            {
                return;
            }

            _registeredCameras.RemoveAt(index);

            if (_activeCamera == identity.Camera)
            {
                _activeCamera = null;

                if (_registeredCameras.Count > 0)
                {
                    int fallbackIndex = Mathf.Min(index, _registeredCameras.Count - 1);
                    ActivateCamera(_registeredCameras[fallbackIndex]);
                }
            }
        }

        /// <summary>
        /// Cycles to the next registered camera, in Order. This is the only thing
        /// PlayerCameraSwitcher needs to call - it never touches priorities itself.
        /// </summary>
        public void SwitchToNext()
        {
            if (_registeredCameras.Count == 0)
            {
                LogX.LogWarning("No cameras registered", this);
                return;
            }

            int currentIndex = -1;

            for (int i = 0; i < _registeredCameras.Count; i++)
            {
                if (_registeredCameras[i].Camera == _activeCamera)
                {
                    currentIndex = i;
                    break;
                }
            }

            int nextIndex = (currentIndex + 1) % _registeredCameras.Count;
            ActivateCamera(_registeredCameras[nextIndex]);
        }


        public void ActivateCamera(CameraIdentity identity)
        {
            if (_activeCamera != null)
            {
                _activeCamera.Priority = InactiveCameraPriority;
            }

            identity.Camera.Priority = ActiveCameraPriority;
            _activeCamera = identity.Camera;

            LogX.LogDebug($"[{this.GetType().Name}]: {identity.name} is activated");
        }
    }
}