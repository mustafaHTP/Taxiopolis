using Game.Systems.PedestrianSystem.Pushing;
using Game.Utils;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Systems.PedestrianSystem
{
    public class Pedestrian : MonoBehaviour, IPushable
    {
        [Header("Debug")]
        [SerializeField] private bool _debugDrawDestination;
        [SerializeField] private Color _debugDestinationColor = Color.green;

        [Header("Settings")]
        [SerializeField] private LayerMask _obstacleLayerMask;
        [SerializeField] private PedestrianPoint _currentWaypoint;
        [SerializeField] private float _destinationReachThreshold;
        [SerializeField] private float _moveSpeed = 1f;
        [SerializeField] private float _rotationSpeed = 5f;

        [Header("Push Settings")]
        [SerializeField] private float _pushMovementSpeed;
        [SerializeField] private float _pushRotationSpeed;

        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private CapsuleCollider _capsuleCollider;

        private const string IdleAnimationName = "Idle";
        private const string WalkAnimationName = "Walk";

        private Vector3 _targetPosition;
        private Direction _direction = Direction.Forward;
        private Vector3 _pushVector;
        private Vector3 _pushDestination;
        private bool _hasPushed;

        // Animation Hashes
        private readonly int _idleAnimationHash = Animator.StringToHash(IdleAnimationName);
        private readonly int _walkAnimationHash = Animator.StringToHash(WalkAnimationName);

        private enum Direction
        {
            Forward,
            Backward
        }

        public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }

        public void Push(Vector3 pushVector)
        {
            SetPushDestination(pushVector);
        }

        public void Reset()
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            _currentWaypoint = null;
            _targetPosition = Vector3.zero;
            _direction = Direction.Forward;
        }

        public void SetWaypoint(PedestrianPoint waypoint)
        {
            if (waypoint == null)
            {
                LogX.LogError("Attempted to set a null waypoint on Pedestrian.");

                return;
            }

            _currentWaypoint = waypoint;
            _targetPosition = _currentWaypoint.GetRandomPosition();
        }

        private void OnDrawGizmos()
        {
            if (_debugDrawDestination && _currentWaypoint != null)
            {
                Gizmos.color = _debugDestinationColor;
                Gizmos.DrawWireSphere(_currentWaypoint.transform.position,
                    _destinationReachThreshold);
                Gizmos.DrawLine(transform.position, _currentWaypoint.transform.position);
            }
        }

        private void Start()
        {
            _direction = GetRandomDirection();

            if (_currentWaypoint != null)
            {
                _targetPosition = _currentWaypoint.GetRandomPosition();
            }
        }

        private void Update()
        {
            if (_currentWaypoint == null) return;

            if (_hasPushed)
            {
                if (HasReachedPushDestination())
                {
                    ClearPushDestination();
                }
                else
                {
                    Vector3 directionTarget = _pushDestination - transform.position;
                    Vector3 moveDelta = _pushMovementSpeed * Time.deltaTime * directionTarget.normalized;
                    if (IsPathBlocked(moveDelta, _pushMovementSpeed))
                    {
                        // Stop pushing if path is blocked
                        ClearPushDestination();

                        return;
                    }

                    MoveAndRotate(
                        _pushDestination,
                        _pushMovementSpeed,
                        _pushRotationSpeed);
                }

                return;
            }

            if (HasReachedDestination())
            {
                SetNextWaypoint();
            }
            else
            {
                MoveAndRotate(
                    _targetPosition,
                    _moveSpeed,
                    _rotationSpeed);
            }
        }

        private Direction GetRandomDirection()
        {
            var directionArray = Enum.GetValues(typeof(Direction));
            var randomIndex = Random.Range(0, directionArray.Length);
            var randomDirectionValue = (Direction)directionArray.GetValue(randomIndex);
            return randomDirectionValue;
        }

        private bool IsPathBlocked(Vector3 moveDelta, float movementSpeed)
        {
            return Physics.Raycast(
                transform.position + transform.up * _capsuleCollider.height,
                moveDelta.normalized,
                movementSpeed * Time.deltaTime,
                _obstacleLayerMask);
        }

        private void MoveAndRotate(Vector3 target, float movementSpeed, float rotationSpeed)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    target - transform.position,
                    Vector3.up);
            Quaternion currentRotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed);

            Vector3 currentPosition = Vector3.MoveTowards(
                transform.position,
                target,
                 Time.deltaTime * movementSpeed);

            transform.SetPositionAndRotation(currentPosition, currentRotation);
        }

        private void SetNextWaypoint()
        {
            bool shouldFollowBranch = false;
            if (_currentWaypoint.Branches.Count > 0)
            {
                float branchingProbability = Random.Range(0f, 1f);
                shouldFollowBranch = branchingProbability <= _currentWaypoint.BranchProbability;
            }

            PedestrianPoint nextWaypoint;
            if (shouldFollowBranch)
            {
                int randomBranchIndex = Random.Range(0, _currentWaypoint.Branches.Count);
                nextWaypoint = _currentWaypoint.Branches[randomBranchIndex];
            }
            else
            {
                if (_direction == Direction.Forward)
                {
                    nextWaypoint = _currentWaypoint.Next;
                }
                else
                {
                    nextWaypoint = _currentWaypoint.Previous;
                }
            }

            // If pedestrian reached the end of the path, try to reverse direction
            if (nextWaypoint == null)
            {
                if (_direction == Direction.Forward && _currentWaypoint.Previous != null)
                {
                    nextWaypoint = _currentWaypoint.Previous;
                    _direction = Direction.Backward;
                }
                else if (_direction == Direction.Backward && _currentWaypoint.Next != null)
                {
                    nextWaypoint = _currentWaypoint.Next;
                    _direction = Direction.Forward;
                }
            }

            _currentWaypoint = nextWaypoint;

            if (_currentWaypoint == null)
            {
                _animator.CrossFade(_idleAnimationHash, 0.1f);
            }
            else
            {
                _targetPosition = _currentWaypoint.GetRandomPosition();
                _animator.CrossFade(_walkAnimationHash, 0.1f);
                LogX.LogDebug("Pedestrian moving to next waypoint.");
            }

            return;
        }

        private bool HasReachedPushDestination()
        {
            Vector3 directionToPushDestination = _pushDestination - transform.position;

            return directionToPushDestination.sqrMagnitude <= _destinationReachThreshold * _destinationReachThreshold;
        }

        private bool HasReachedDestination()
        {
            Vector3 directionToDestination = _targetPosition - transform.position;

            return directionToDestination.sqrMagnitude <= _destinationReachThreshold * _destinationReachThreshold;
        }

        private void SetPushDestination(Vector3 pushVector)
        {
            _pushVector = pushVector;
            _pushDestination = transform.position + _pushVector;
            _hasPushed = true;
        }

        private void ClearPushDestination()
        {
            _pushVector = Vector3.zero;
            _pushDestination = Vector3.zero;
            _hasPushed = false;
        }
    }
}
