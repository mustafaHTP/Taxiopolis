using Framework.Services;
using Game.Utils;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Systems.CameraSystem
{
    public class CameraImpulseManager : MonoBehaviour, IGlobalService
    {
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        public void PlayImpulse(ImpulseDefinition definition)
        {
            _impulseSource.ImpulseDefinition.AmplitudeGain = definition.Amplitude;
            _impulseSource.ImpulseDefinition.AmplitudeGain = definition.Frequency;
            _impulseSource.ImpulseDefinition.ImpulseShape = definition.Shape;
            _impulseSource.ImpulseDefinition.ImpulseDuration = definition.Duration;
            _impulseSource.GenerateImpulse(definition.Velocity * definition.Force);
        }

        public void AfterInit()
        {
            // noop
        }

        public void Dispose()
        {
            // noop
        }

        public void Init()
        {
            // noop
        }
    }
}
