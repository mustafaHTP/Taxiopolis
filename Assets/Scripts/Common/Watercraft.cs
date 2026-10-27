using Game.Utils;
using UnityEngine;

namespace Game.Common
{
    public class Watercraft : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _debugDrawWaypoint;
        [SerializeField] private Color _debugWaypointColor = Color.red;
        [SerializeField] private float _debugWaypointRadius = 2f;

        [Header("Settings")]
        [SerializeField] private WaypointRoute _waypointRoute;
        [SerializeField] private float _speed;
        [SerializeField] private float _turnSpeed;
        [SerializeField] private float _arrivalThreshold = 0.1f;
        [SerializeField] private float _waveHeight = 0.5f;
        [SerializeField] private float _waveSpeed = 1.0f;

        private Waypoint _currentWaypoint;
        private Vector3 _initialPosition;
        private bool _hasPath;

        private void OnDrawGizmos()
        {
            if (_debugDrawWaypoint && _currentWaypoint != null)
            {
                Gizmos.color = _debugWaypointColor;
                Gizmos.DrawLine(transform.position, _currentWaypoint.Position);
                Gizmos.DrawSphere(_currentWaypoint.Position, _debugWaypointRadius);
            }
        }

        private void Awake()
        {
            _initialPosition = transform.position;
        }

        private void Start()
        {
            if (_waypointRoute == null)
            {
                LogX.LogWarning("Watercraft has no waypoint route.", this);

                return;
            }

            Waypoint startingWaypoint = _waypointRoute.GetStartingWaypoint();
            if (startingWaypoint == null)
            {
                LogX.LogWarning($"Starting waypoint is null for Watercraft-${name}", this);

                _hasPath = false;
            }
            else
            {
                _hasPath = true;
            }

            _currentWaypoint = startingWaypoint;
        }

        private void Update()
        {
            ApplyWaveMotion();

            if (!_hasPath) return;

            MoveWaypoint();

            if (HasArriveWaypoint())
            {
                SetNextWaypoint();
            }
        }

        private void MoveWaypoint()
        {
            // --- ROTATION ---
            Vector3 flatTargetPos = new(
                _currentWaypoint.Position.x,
                transform.position.y, _currentWaypoint.Position.z);
            Quaternion newRotation = Quaternion.LookRotation(flatTargetPos - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                newRotation,
                _turnSpeed * Time.deltaTime);

            // --- MOVEMENT (XZ only) ---
            Vector3 direction = (flatTargetPos - transform.position).normalized;
            transform.position += _speed * Time.deltaTime * direction;
        }

        private void ApplyWaveMotion()
        {
            float positionY = Mathf.Sin(Time.time * _waveSpeed) * _waveHeight;

            Vector3 pos = transform.position;
            pos.y = _initialPosition.y + positionY;
            transform.position = pos;
        }

        private void SetNextWaypoint()
        {
            var nextWaypoint = _waypointRoute.GetNextWaypoint(_currentWaypoint);
            if (nextWaypoint == null)
            {
                _hasPath = false;
                _currentWaypoint = null;

                return;
            }

            _currentWaypoint = nextWaypoint;
        }

        private bool HasArriveWaypoint()
        {
            Vector3 waypointPosition = _currentWaypoint.Position;
            Vector3 directionToWaypoint = waypointPosition - transform.position;

            return directionToWaypoint.sqrMagnitude <= _arrivalThreshold * _arrivalThreshold;
        }
    }
}
