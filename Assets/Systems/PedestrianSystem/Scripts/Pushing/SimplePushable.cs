using Game.Utils;
using UnityEngine;

namespace Game.Systems.PedestrianSystem.Pushing
{
    public class SimplePushable : MonoBehaviour, IPushable
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _arrivalThreshold = 0.1f;

        private Vector3 _pushVector;
        private Vector3 _destination;
        private bool _hasDestination = false;

        public void Push(Vector3 pushVector)
        {
            _pushVector = pushVector;
            SetDestination(transform.position + _pushVector);
        }

        private void SetDestination(Vector3 destination)
        {
            _destination = destination;
            _hasDestination = true;

            LogX.LogDebug($"[SimplePushable] New destination set to {_destination} for {gameObject.name}");
        }

        private void ClearDestination()
        {
            _destination = Vector3.zero;
            _hasDestination = false;
        }

        private void Update()
        {
            if (!_hasDestination) return;
            if (_pushVector.sqrMagnitude < 0.001f) return;

            Vector3 direction = _destination - transform.position;

            if (direction.magnitude < _arrivalThreshold)
            {
                _pushVector = Vector3.zero;
                ClearDestination();
                return;
            }

            Vector3 moveDelta = direction.normalized * _moveSpeed * Time.deltaTime;
            transform.position += moveDelta;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _destination);
            Gizmos.DrawWireSphere(_destination, 1f);
        }
    }
}
