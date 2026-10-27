using Game.Utils;
using UnityEngine;

namespace Game.Common
{
    public class Aircraft : MonoBehaviour
    {
        [SerializeField] private WaypointRoute _waypointRoute;
        [SerializeField] private bool _isLooping;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _turnSpeed = 2f;
        [SerializeField] private float _arrivalThreshold = 0.5f;

        private bool _hasCompletedPath = false;
        private Waypoint _currentWaypoint;

        private void Start()
        {
            if (_waypointRoute == null)
            {
                LogX.LogWarning("Aircraft has no waypoint route.", this);

                return;
            }

            Waypoint startingWaypoint = _waypointRoute.GetStartingWaypoint();
            if (startingWaypoint == null)
            {
                LogX.LogWarning($"Starting waypoint is null for Aircraft-${name}", this);

                return;
            }

            _currentWaypoint = startingWaypoint;

            // Warp the starting waypoint
            transform.SetPositionAndRotation(
                _currentWaypoint.Position,
                _currentWaypoint.Rotation);
        }

        private void Update()
        {
            if (_hasCompletedPath || _currentWaypoint == null) return;

            if (HasArrivedAtWaypoint())
            {
                AdvanceWaypoint();
                if (_hasCompletedPath) return;
            }

            MoveTowardWaypoint();
        }

        private void MoveTowardWaypoint()
        {
            // first rotate
            Vector3 direction = (_currentWaypoint.Position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);

            // then move
            transform.position += transform.forward * _speed * Time.deltaTime;
        }

        private void AdvanceWaypoint()
        {
            if (_waypointRoute.IsEndOfRoute(_currentWaypoint))
            {
                if (_isLooping) _currentWaypoint = _waypointRoute.GetStartingWaypoint();
                else _hasCompletedPath = true;
            }
            else
            {
                _currentWaypoint = _waypointRoute.GetNextWaypoint(_currentWaypoint);
            }
        }

        private bool HasArrivedAtWaypoint()
        {
            return (transform.position - _currentWaypoint.Position).sqrMagnitude <=
                   _arrivalThreshold * _arrivalThreshold;
        }
    }
}