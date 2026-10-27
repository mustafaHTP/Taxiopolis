using Game.Utils;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Common
{
    public class PlatformVolumeConfigurator : MonoBehaviour
    {
        [SerializeField] private Volume _volume;
        [SerializeField] private PlatformVolumeEntry[] _platformVolumeEntries;

        private void Awake()
        {
            if(_volume == null)
            {
                LogX.LogError("Volume is not assigned in PlatformVolumeConfigurator.");

                return;
            }

            if(_platformVolumeEntries == null || _platformVolumeEntries.Length == 0)
            {
                LogX.LogError("No platform volume entries assigned in PlatformVolumeConfigurator.");

                return;
            }

            CustomPlatform currentPlatform = PlatformUtils.GetPlatform();
            for (int i = 0; i < _platformVolumeEntries.Length; i++)
            {
                PlatformVolumeEntry entry = _platformVolumeEntries[i];
                if (entry.Platform == currentPlatform)
                {
                    _volume.sharedProfile = entry.VolumeProfile;

                    break;
                }
            }
        }
    }

    [Serializable]
    public class PlatformVolumeEntry
    {
        [SerializeField] private CustomPlatform _platform;
        [SerializeField] private VolumeProfile _volumeProfile;

        public CustomPlatform Platform => _platform;
        public VolumeProfile VolumeProfile => _volumeProfile;
    }
}
