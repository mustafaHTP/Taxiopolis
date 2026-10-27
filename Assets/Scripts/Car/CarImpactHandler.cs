using Framework.Services;
using Game.Systems.CameraSystem;
using Game.Systems.SoundSystem;
using Game.Utils;
using System;
using UnityEngine;

namespace Game.Car
{
    public class CarImpactHandler : MonoBehaviour
    {
        public event Action<ImpactInfo> OnImpactDetected;

        [SerializeField]
        [Tooltip("IMPORTANT: Sort by minimum relative velocity in descending order. Incorrect ordering may cause impact detection to select the wrong impact data.")]
        private ImpactData[] _impactDataArray;

        public enum ImpactType
        {
            None,
            Minor,
            Major
        }

        public readonly struct ImpactInfo
        {
            public readonly ImpactType ImpactType;
            public readonly Vector3 Position;
            public readonly Vector3 Normal;

            public ImpactInfo(ImpactType impactType, Vector3 position, Vector3 normal)
            {
                ImpactType = impactType;
                Position = position;
                Normal = normal;
            }
        }

        [Serializable]
        private class ImpactRule
        {
            [SerializeField] private ImpactType _impactType;
            [SerializeField] private float _minRelativeVelocity;
            [SerializeField] private ImpulseDefinition _impulseDefinition;

            public ImpactType ImpactType => _impactType;
            public float MinRelativeVelocity => _minRelativeVelocity;
            public ImpulseDefinition ImpulseDefinition => _impulseDefinition;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Vector3 collisionNormalAvg = Vector3.zero;
            for (int i = 0; i < collision.contactCount; i++)
            {
                collisionNormalAvg += collision.GetContact(i).normal;
            }

            collisionNormalAvg = collisionNormalAvg.normalized;

            if (Vector3.Dot(collisionNormalAvg, Vector3.up) > 0.5f)
            {
                // Ignore collisions from below
                return;
            }

            if (_impactDataArray == null || _impactDataArray.Length == 0) return;

            // calculate that collision falls into which impact data
            float relativeVelocity = collision.relativeVelocity.magnitude;
            ImpactData impactData = null;
            for (int i = 0; i < _impactDataArray.Length; i++)
            {
                if (relativeVelocity >= _impactDataArray[i].MinRelativeVelocity)
                {
                    impactData = _impactDataArray[i];
                    break;
                }
            }

            // there is no impact data for that collision skip it
            if (impactData == null) return;

            ImpactInfo impactInfo = new(
                    impactData.Type,
                    collision.GetContact(0).point,
                    collision.GetContact(0).normal);

            Instantiate(
                impactData.Vfx,
                impactInfo.Position,
                Quaternion.LookRotation(impactInfo.Normal));
            ServiceLocator
                .Global
                .GetService<SoundManager>()
                .PlaySound(impactData.SoundData);
            ServiceLocator
                .Global
                .GetService<CameraImpulseManager>()
                .PlayImpulse(impactData.ImpulseDefinition);

            OnImpactDetected?
                .Invoke(impactInfo);

            LogX.LogDebug($"Car collided with {collision.gameObject.name}, Relative Velocity: {relativeVelocity}, Impact Type: {impactData.Type}");
        }
    }
}