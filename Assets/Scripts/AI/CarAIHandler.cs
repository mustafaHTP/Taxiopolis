using Framework.FSM;
using Game.AI.FSM;
using Game.Car;
using Game.Input;
using Game.Utils;
using RoadSystem;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using LogX = Game.Utils.LogX;
using Random = UnityEngine.Random;

namespace Game.AI
{
    [RequireComponent(typeof(CarController))]
    public class CarAIHandler : MonoBehaviour, ICarInputProvider
    {
        [Header("Debug")]
        [SerializeField] private bool _debugDrawWaypoint = true;
        [SerializeField] private bool _debugDrawTrafficCheck = true;
        [SerializeField] private Color _debugTrafficCheckRayColor = Color.green;
        [SerializeField] private bool _debugDrawTurningCircles = true;
        [SerializeField] private Color _debugTurningCircleColor = Color.yellow;
        [SerializeField] private Color _debugExitTurningCircleColor = Color.yellow;
        [SerializeField] private Color _debugEnterTurningCircleColor = Color.red;
        [SerializeField] private Vector3 _debugTurningCircleCenterOffset;
        [SerializeField] private int _debugTurningCircleSegments = 24;

        [Header("AI Settings")]
        [SerializeField] private RoadPoint _currentRoadPoint;
        [SerializeField] private bool _enableTrafficCheck = true;
        [SerializeField] private float _trafficCheckDistance = 20f;
        [SerializeField] private Vector3 _trafficCheckOffset = Vector3.zero;
        [SerializeField] private int _trafficCheckRayCount = 5;
        [SerializeField] private LayerMask _trafficCheckLayer;
        [SerializeField] private float _trafficCheckRaySpreadAngle = 30f;
        [SerializeField] private float _thresholdToEnterAndExitTurnRadius = 0.2f;
        [SerializeField] private float _trafficLightStopDistance = 10f;
        [SerializeField] private float _forwardDotResultThresholdToChangeDirection = 0.15f;
        [SerializeField]
        [Tooltip("Distance to waypoint to consider it reached and look for next waypoint")]
        private float _waypointArrivalDistance = 2f;

        /// <summary>
        /// Represents the turning radius of the vehicle, typically calculated based on steering angle, wheelbase, and
        /// track width. It is calculated based on steering angle, wheelbase and track width
        /// </summary>
        private float _turningRadius = 0f;

        #region Input Provider Implementation
        public FrameInput FrameInput { get; private set; }
        public InputAction GasAction => null;
        public InputAction BrakeAction => null;
        public InputAction SteerAction => null;
        public InputAction HandbrakeAction => null;
        public InputAction JumpAction => null;
        public InputAction ResetCarRotation => null;
        public InputAction PitchAction => null;
        public InputAction RollAction => null;
        public InputAction HornAction => null;
        public InputAction UseNitroAction => null;
        public InputAction UseShockwaveAction => null;
        public InputAction UseSpeedbreakerAction => null;
        public InputAction RecoverAction => null;

        public InputAction FindAction(string actionMap, string actionName)
        {
            return null;
        }

        #endregion

        private CarController _carController;
        private StateMachine _stateMachine;
        private CarAIContext _carAIContext;

        public void SetInputs(float gasInput, float brakeInput, float steerInput)
        {
            FrameInput = new FrameInput(
                gasInput,
                brakeInput,
                steerInput,
                0f);
        }

        public void SetRoadPoint(RoadPoint roadPoint)
        {
            if (roadPoint == null)
            {
                LogX.LogWarning("CarAIHandler: SetRoadPoint called with null RoadPoint.");
                return;
            }

            _currentRoadPoint = roadPoint;
        }

        private void OnDrawGizmos()
        {
            if (_debugDrawWaypoint && _currentRoadPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _currentRoadPoint.Position);
                Gizmos.DrawWireSphere(_currentRoadPoint.Position, _waypointArrivalDistance);
            }

            if (_debugDrawTurningCircles)
            {
                if (!TryGetComponent(out CarController carController)) return;

                // Calculate turning radius based on steering angle
                float maxSteeringAngle = carController.CarData.SteeringAngle;
                float wheelBase = carController.WheelBase;
                float trackWidth = carController.TrackWidth;

                float turningRadius = trackWidth * 0.5f + wheelBase / Mathf.Sin(Mathf.Deg2Rad * maxSteeringAngle);


                Vector3 turningCircleCenter = transform.position + _debugTurningCircleCenterOffset;
                Vector3 turningCircleCenterOffset =
                    transform.forward * _debugTurningCircleCenterOffset.z
                    + transform.right * _debugTurningCircleCenterOffset.x
                    + transform.up * _debugTurningCircleCenterOffset.y;

                turningCircleCenter += turningCircleCenterOffset;

                // Draw right turning circle
                Gizmos.color = _debugTurningCircleColor;
                DrawX
                    .Gizmos
                    .DrawCircle(
                    turningCircleCenter + transform.right * turningRadius,
                    transform.up,
                    transform.right,
                   turningRadius,
                    _debugTurningCircleSegments);

                // Draw right exit turning circle
                Gizmos.color = _debugExitTurningCircleColor;
                DrawX
                    .Gizmos
                    .DrawCircle(
                    turningCircleCenter + transform.right * turningRadius,
                    transform.up,
                    transform.right,
                   turningRadius + _thresholdToEnterAndExitTurnRadius,
                    _debugTurningCircleSegments);

                // Draw right enter turning circle
                Gizmos.color = _debugEnterTurningCircleColor;
                DrawX
                   .Gizmos
                   .DrawCircle(
                   turningCircleCenter + transform.right * turningRadius,
                   transform.up,
                   transform.right,
                  turningRadius - _thresholdToEnterAndExitTurnRadius,
                   _debugTurningCircleSegments);

                // Draw left turning circle
                Gizmos.color = _debugTurningCircleColor;
                DrawX
                    .Gizmos
                    .DrawCircle(
                    turningCircleCenter - transform.right * turningRadius,
                    transform.up,
                    transform.right,
                   turningRadius,
                    _debugTurningCircleSegments);

                // Draw left exit turning circle
                Gizmos.color = _debugExitTurningCircleColor;
                DrawX
                    .Gizmos
                    .DrawCircle(
                    turningCircleCenter - transform.right * turningRadius,
                    transform.up,
                    transform.right,
                   turningRadius + _thresholdToEnterAndExitTurnRadius,
                    _debugTurningCircleSegments);

                // Draw left enter turning circle
                Gizmos.color = _debugEnterTurningCircleColor;
                DrawX
                   .Gizmos
                   .DrawCircle(
                   turningCircleCenter - transform.right * turningRadius,
                   transform.up,
                   transform.right,
                  turningRadius - _thresholdToEnterAndExitTurnRadius,
                   _debugTurningCircleSegments);
            }

            if (_debugDrawTrafficCheck)
            {
                Gizmos.color = _debugTrafficCheckRayColor;
                Vector3 trafficCheckOrigin = transform.position + _trafficCheckOffset;

                if (_trafficCheckRaySpreadAngle <= 0f || _trafficCheckRayCount <= 1)
                {
                    Gizmos.DrawRay(trafficCheckOrigin, transform.forward * _trafficCheckDistance);
                    return;
                }

                float angleStep = _trafficCheckRaySpreadAngle / (_trafficCheckRayCount - 1);
                float startAngle = -_trafficCheckRaySpreadAngle / 2;

                for (int i = 0; i < _trafficCheckRayCount; i++)
                {
                    float angle = startAngle + i * angleStep;
                    Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
                    Vector3 direction = rotation * transform.forward;
                    Gizmos.DrawRay(trafficCheckOrigin, direction * _trafficCheckDistance);
                }
            }
        }

        private void Awake()
        {
            _carController = GetComponent<CarController>();
            _turningRadius = GetTurningRadius();

            _carAIContext = new(_carController, this);

            _stateMachine = new();

            var moveForwardState = new MoveForwardState("MoveForward", _carAIContext);
            var moveBackwardState = new MoveBackwardState("MoveBackward", _carAIContext);
            var stopState = new StopState("Stop", _carAIContext);
            var turnCircleIntrusionState = new TurnCircleIntrusionState("TurnCircleIntrusion", _carAIContext);

            // Stop State Transitions
            _stateMachine.AddAnyTransition(
                stopState, new FuncPredicate(() => _carAIContext.IsTrafficLightStop || _currentRoadPoint == null || _carAIContext.IsTrafficBlock));
            _stateMachine.AddTransition(
                stopState,
                moveForwardState,
                new FuncPredicate(() => _carAIContext.ForwardDotResult >= 0f));
            _stateMachine.AddTransition(
                stopState,
                                moveBackwardState,
                new FuncPredicate(() => _carAIContext.ForwardDotResult < 0f));

            // Turn Circle Intrusion State Transitions
            _stateMachine.AddAnyTransition(
                turnCircleIntrusionState, new FuncPredicate(() => _carAIContext.IsInsideLeftTurningCircle || _carAIContext.IsInsideRightTurningCircle));
            _stateMachine.AddTransition(
                turnCircleIntrusionState,
                moveForwardState,
                new FuncPredicate(() => _carAIContext.IsOutSideLeftTurningCircle && _carAIContext.IsOutSideRightTurningCircle && _carAIContext.ForwardDotResult >= 0f));
            _stateMachine.AddTransition(
                turnCircleIntrusionState,
                moveBackwardState,
                new FuncPredicate(() => _carAIContext.IsOutSideLeftTurningCircle && _carAIContext.IsOutSideRightTurningCircle && _carAIContext.ForwardDotResult < 0f));

            // Move Forward State Transitions
            _stateMachine.AddTransition(
                moveForwardState,
                moveBackwardState,
                new FuncPredicate(() => _carAIContext.ForwardDotResult <= 0 - _forwardDotResultThresholdToChangeDirection));

            // Move Backward State Transitions
            _stateMachine.AddTransition(
                moveBackwardState,
                moveForwardState,
                new FuncPredicate(() => _carAIContext.ForwardDotResult >= 0 + _forwardDotResultThresholdToChangeDirection));

            _stateMachine.SetState(stopState);
        }

        private void Start()
        {
            UpdateAIContext();

            _stateMachine.Init();
        }

        private void Update()
        {
            UpdateAIContext();

            _stateMachine.Update();
        }

        private float GetTurningRadius()
        {
            float maxSteeringAngle = _carController.CarData.SteeringAngle;
            float wheelBase = _carController.WheelBase;
            float trackWidth = _carController.TrackWidth;
            float turningRadius = trackWidth * 0.5f + wheelBase / Mathf.Sin(Mathf.Deg2Rad * maxSteeringAngle);

            return turningRadius;
        }

        private void UpdateAIContext()
        {
            // No need to make calculations
            if (_currentRoadPoint == null)
            {
                return;
            }

            // Check for red light
            float distanceSqrTrafficLight = (_currentRoadPoint.transform.position - transform.position).sqrMagnitude;
            if (!_currentRoadPoint.IsAllowedToDrive
                && distanceSqrTrafficLight < _trafficLightStopDistance * _trafficLightStopDistance)
            {
                _carAIContext.IsTrafficLightStop = true;
            }
            else
            {
                _carAIContext.IsTrafficLightStop = false;
            }

            // Traffic check
            if (_enableTrafficCheck)
            {
                Vector3 trafficCheckOrigin = transform.position + _trafficCheckOffset;

                if (_trafficCheckRaySpreadAngle <= 0f || _trafficCheckRayCount <= 1)
                {
                    // Single raycast
                    if (Physics.Raycast(
                        trafficCheckOrigin,
                        transform.forward,
                        out RaycastHit hitInfo,
                        _trafficCheckDistance,
                        _trafficCheckLayer))
                    {
                        _carAIContext.IsTrafficBlock = true;
                    }
                    else
                    {
                        _carAIContext.IsTrafficBlock = false;

                    }
                }
                else
                {
                    bool hasDetectedTraffic = false;
                    // Multiple raycasts in a spread
                    float angleStep = _trafficCheckRaySpreadAngle / (_trafficCheckRayCount - 1);
                    float startAngle = -_trafficCheckRaySpreadAngle / 2;
                    for (int i = 0; i < _trafficCheckRayCount; i++)
                    {
                        float angle = startAngle + i * angleStep;
                        Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
                        Vector3 direction = rotation * transform.forward;
                        if (Physics.Raycast(
                            trafficCheckOrigin,
                            direction,
                            out RaycastHit hitInfo,
                            _trafficCheckDistance,
                            _trafficCheckLayer))
                        {
                            hasDetectedTraffic = true;
                            break;
                        }
                    }

                    if (hasDetectedTraffic)
                    {
                        _carAIContext.IsTrafficBlock = true;
                    }
                    else
                    {
                        _carAIContext.IsTrafficBlock = false;
                    }
                }
            }
            else
            {
                _carAIContext.IsTrafficBlock = false;
            }

            // Update target speed based on current road point
            _carController.SetTargetSpeed(_currentRoadPoint.SpeedLimit);

            // First check target point is inside of turning circles
            Vector3 turningCircleCenter = transform.position;
            Vector3 turningCircleLeftCenter =
                turningCircleCenter - transform.right * _turningRadius;
            Vector3 turningCircleRightCenter =
                turningCircleCenter + transform.right * _turningRadius;

            Vector3 directionToRoadPoint = _currentRoadPoint.transform.position - transform.position;

            // Project road point on turning circle plane
            Vector3 projectedRoadPoint = Vector3.ProjectOnPlane(
                directionToRoadPoint,
                transform.up);

            projectedRoadPoint += turningCircleCenter;

            Vector3 diffToLeftCenter = projectedRoadPoint - turningCircleLeftCenter;
            Vector3 diffToRightCenter = projectedRoadPoint - turningCircleRightCenter;
            float turningRadiusSqrToEnterTurningCircle = Mathf.Pow((_turningRadius - _thresholdToEnterAndExitTurnRadius), 2);
            float turningRadiusSqrToExitTurningCircle = Mathf.Pow((_turningRadius + _thresholdToEnterAndExitTurnRadius), 2);

            _carAIContext.IsInsideLeftTurningCircle = diffToLeftCenter.sqrMagnitude < turningRadiusSqrToEnterTurningCircle;
            _carAIContext.IsInsideRightTurningCircle = diffToRightCenter.sqrMagnitude < turningRadiusSqrToEnterTurningCircle;

            _carAIContext.IsOutSideLeftTurningCircle = diffToLeftCenter.sqrMagnitude > turningRadiusSqrToExitTurningCircle;
            _carAIContext.IsOutSideRightTurningCircle = diffToRightCenter.sqrMagnitude > turningRadiusSqrToExitTurningCircle;

            // Steer input calculation
            // Both vector must be normalized to get correct dot product result
            _carAIContext.RightDotResult = Vector3.Dot(
                transform.right,
                directionToRoadPoint.normalized);

            // Gas input calculation
            // Both vector must be normalized to get correct dot product result
            _carAIContext.ForwardDotResult = Vector3.Dot(
                transform.forward,
                directionToRoadPoint.normalized);

            _carAIContext.RightDotResult = Mathf.Clamp(_carAIContext.RightDotResult, -1f, 1f);
            _carAIContext.ForwardDotResult = Mathf.Clamp(_carAIContext.ForwardDotResult, -1f, 1f);

            if (IsWaypointReached())
            {
                SetNextWaypoint();
            }
        }

        private bool IsWaypointReached()
        {
            if (_currentRoadPoint == null) return false;

            float distanceToWaypoint = Vector3.Distance(
                transform.position,
                _currentRoadPoint.transform.position);

            return distanceToWaypoint < _waypointArrivalDistance;
        }

        private void SetNextWaypoint()
        {
            // look for next waypoint of current road
            RoadPoint nextRoadPoint = null;
            if (_currentRoadPoint.GetNext() != null)
            {
                nextRoadPoint = _currentRoadPoint.GetNext();
            }
            else if (_currentRoadPoint.ConnectionsCount > 0)
            {
                // pick random connection
                nextRoadPoint =
                    _currentRoadPoint
                        .Connections
                        .ElementAt(Random.Range(0, _currentRoadPoint.ConnectionsCount));
            }

            if (nextRoadPoint != null)
            {
                _currentRoadPoint = nextRoadPoint;

                _carController.SetTargetSpeed(_currentRoadPoint.SpeedLimit);
            }
            else
            {
                _currentRoadPoint = null;
                _carController.SetTargetSpeed(0f);

                LogX.LogWarning($"CarAIHandler: No next RoadPoint found for {gameObject.name}");
            }
        }
    }
}
