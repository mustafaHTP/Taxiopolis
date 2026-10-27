using Unity.Cinemachine;
using UnityEngine;

namespace Game.Systems.CameraSystem
{
    [CreateAssetMenu(fileName = "Impulse Definition", menuName = "Game/Camera System/Impulse Definition")]
    public class ImpulseDefinition : ScriptableObject
    {
        public float Amplitude;
        public float Frequency;
        public float Duration;
        public Vector3 Velocity;
        public float Force;
        public CinemachineImpulseDefinition.ImpulseShapes Shape;
    }
}
