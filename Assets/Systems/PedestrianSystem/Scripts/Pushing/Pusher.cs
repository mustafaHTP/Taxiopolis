using Game.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Game.Systems.PedestrianSystem.Pushing
{
    public class Pusher : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _debugDrawPushRadius = true;
        [SerializeField] private Color _debugPushRadiusColor = Color.cyan;

        [Header("Settings")]
        [SerializeField] private float _pushRadius = 5f;
        [SerializeField] private float _pushOffset = 1f;
        [SerializeField] private float _pushSpeedThreshold = 0.1f;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            if (!TryGetComponent(out _rigidbody))
            {
                LogX.LogError($"Pusher component on {gameObject.name} requires a Rigidbody component.");

                return;
            }
        }

        private void Update()
        {
            if (_rigidbody.linearVelocity.magnitude < _pushSpeedThreshold) return;

            var colliders = Physics.OverlapSphere(transform.position, _pushRadius);
            if (colliders.Length == 0) return;
            Vector2 planarPosition = new Vector2(transform.position.x, transform.position.z);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].TryGetComponent(out IPushable pushable))
                {
                    Vector2 pushablePlanarPosition = new(
                        colliders[i].transform.position.x,
                        colliders[i].transform.position.z);
                    Vector2 directionPushable = pushablePlanarPosition - planarPosition;
                    float distancePushable = directionPushable.magnitude;
                    if (distancePushable < 0.01f)
                    {
                        continue;
                    }
                    // Always make distance positive
                    float pushDistance = Mathf.Abs(_pushRadius - distancePushable + _pushOffset);
                    Vector2 pushVector = directionPushable * pushDistance;

                    pushable.Push(new Vector3(pushVector.x, 0f, pushVector.y));
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_debugDrawPushRadius)
            {
                Gizmos.color = _debugPushRadiusColor;
                Gizmos.DrawWireSphere(transform.position, _pushRadius);
            }
        }
    }
}
