using Game.Systems.CameraSystem;
using Game.Systems.SoundSystem;
using UnityEngine;
using static Game.Car.CarImpactHandler;

namespace Game.Car
{
    [CreateAssetMenu(fileName = "New Impact Data", menuName = "Game/Impact Data")]
    public class ImpactData : ScriptableObject
    {
        [SerializeField] private ImpactType _type;
        [SerializeField] private float _minRelativeVelocity;
        [SerializeField] private ImpulseDefinition _impulseDefinition;
        [SerializeField] private SoundData _soundData;
        [SerializeField] private ParticleSystem _vfx;

        public ImpactType Type => _type;
        public float MinRelativeVelocity => _minRelativeVelocity;
        public ImpulseDefinition ImpulseDefinition => _impulseDefinition;
        public SoundData SoundData => _soundData;
        public ParticleSystem Vfx => _vfx;
    }
}
