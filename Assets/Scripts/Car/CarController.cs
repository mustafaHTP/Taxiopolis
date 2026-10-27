using Game.Input;
using Game.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Game.Car.CarEnums;
using LogX = Game.Utils.LogX;

namespace Game.Car
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        public event Action OnDriftStarted;
        public event Action OnDriftEnded;
        public event Action OnEngineStarted;

        [Header("Car Data")]
        [SerializeField] private CarData _carData;
        [SerializeField] private bool _initByCarData;

        [Header("Wheels")]
        [SerializeField] private Wheel _fLWheel;
        [SerializeField] private Wheel _fRWheel;
        [SerializeField] private Wheel _rLWheel;
        [SerializeField] private Wheel _rRWheel;

        [Header("Acceleration")]
        [SerializeField] private float _topSpeed = 20f;
        [SerializeField] private float _reverseTopSpeed = 5;
        [SerializeField] private bool _useMotorTorqueCurve;
        [SerializeField] private AnimationCurve _torqueCurve;
        [SerializeField] private Drivetrain _drivetrain;
        [SerializeField] private float _motorTorque = 1000f;
        [SerializeField] private float _brakeForce = 1000f;
        [SerializeField] private float _handBrakeForce = 6000f;
        [SerializeField]
        [Range(0f, 1f)]
        private float _brakeBias = 0.5f; // (front / rear) wheels brake bias
        [SerializeField] private float _forwardFriction = 1000f;
        [SerializeField] private float _normalDrag = 0.1f;
        [SerializeField] private float _stoppingDrag = 100f;
        [SerializeField] private float _stoppingDragSpeedThreshold = 1f;

        [Header("Wheel Mesh")]
        [SerializeField] private float _wheelRadius = 0.5f;
        [SerializeField] private float _wheelSyncPositionSpeed = 10f;
        [SerializeField] private Vector3 _wheelOffset;
        [SerializeField] private float _wheelRollingSpeed = 1f;

        [Header("Suspension")]
        [SerializeField] private float _suspensionStrength = 10000f;
        [SerializeField] private float _suspensionDamping = 1000f;
        [SerializeField] private float _suspensionDistance = 1f;
        [SerializeField] private LayerMask _suspensionCheckLayer;

        [Header("Steering")]
        [Space(5)]
        [SerializeField] private float _fGripStrength = 30f;
        [SerializeField] private float _rGripStrength = 30f;
        [SerializeField] private float _steeringAngle = 10f;
        [SerializeField] private bool _useSteerAngleCurve = false;
        [SerializeField]
        [Tooltip("Steering Angle Curve for smoother steering input based on speed")]
        private AnimationCurve _steeringAngleSpeedCurve;
        [Space(5)]
        [SerializeField] private float _steerSpeed;
        [SerializeField] private bool _useGripCurves = false;
        [SerializeField] private AnimationCurve _fGripCurve;
        [SerializeField] private AnimationCurve _rGripCurve;
        [SerializeField]
        [Range(0, 1)]
        private float _fGripFactor = 1f;
        [SerializeField]
        [Range(0, 1)]
        private float _rGripFactor = 1f;

        [Header("Drift")]
        [Space(5)]
        [SerializeField] private float _driftActivationSpeed = 5f;
        [SerializeField] private float _minSlipAngleForDrift = 10f;
        [SerializeField] private float _maxSlipAngleForDrift = 50f;
        [SerializeField]
        [Range(0, 1)]
        private float _frontWheelDriftGrip = 0.5f;
        [SerializeField]
        [Range(0, 1)]
        private float _backWheelDriftGrip = 0.5f;
        [SerializeField]
        [Tooltip("Torque applied to the body when drift is initiated")]
        private float _driftKickTorque = 5000f;

        [Header("Environment Check")]
        [SerializeField] private bool _isEnvCheckEnabled;
        [SerializeField] private LayerMask _envCheckLayer;
        [SerializeField] private float _groundCheckDistance = 1f;
        [SerializeField] private float _roofCheckDistance = 1f;
        [SerializeField] private float _leftCheckDistance = 1f;
        [SerializeField] private float _rightCheckDistance = 1f;
        [SerializeField] private float _frontCheckDistance = 1f;
        [SerializeField] private float _backCheckDistance = 1f;

        [Header("General")]
        [SerializeField] private bool _useBrakeToGoBack = true;

        [Header("Debug")]
        [Space(5)]
        [SerializeField] private bool _debugDrawEnvRayCheck = false;
        [SerializeField] private bool _debugDrawCarAxes = false;
        [SerializeField] private Color _debugCarAxesColor;
        [Header("Debug COM")]
        [SerializeField] private Transform _comTransform;
        [SerializeField] private bool _drawCOM = true;
        [SerializeField] private Color32 _drawCOMColor;
        [SerializeField] private float _drawCOMRadius = 0.3f;
        [Header("Debug Wheel Axes")]
        [SerializeField] private bool _drawWheelRays = true;
        [SerializeField] private bool _drawWheelAxes = true;
        [SerializeField] private float _wheelAxesLength = 1f;
        [Header("Debug Wheel Sphere")]
        [SerializeField] private bool _drawWheelSphere = true;
        [SerializeField] private float _wheelSphereRadius = 0.5f;
        [SerializeField] private Color _wheelSphereColor = Color.red;
        [SerializeField] private bool _drawCarVelocity = false;

        //References
        private Rigidbody _rigidbody;
        private ICarInputProvider _carInputProvider;

        // Car variables
        private readonly List<IMotorTorqueModifier> _motorTorqueModifiers = new();
        private CarState _carState;
        private bool _isDrifting = false;
        private float _currentSteerAngle = 0;
        private float _previousSpeed = 0f;
        private TorqueDirection _torqueDirection = TorqueDirection.Forward;
        /// <summary>
        /// Used for AI to set a target speed for the car. <br />
        /// </summary>
        private float _targetSpeed = CarConstants.DefaultTargetSpeed;

        #region Properties
        public float WheelBase
        {
            get
            {
                if (_fLWheel == null || _rLWheel == null) return 0f;

                return Vector3.Distance(
                    _fLWheel.WheelPoint.position,
                    _rLWheel.WheelPoint.position);
            }
        }
        public float TrackWidth
        {
            get
            {
                if (_fLWheel == null || _fRWheel == null) return 0f;

                return Vector3.Distance(
                    _fLWheel.WheelPoint.position,
                    _fRWheel.WheelPoint.position);
            }
        }
        public CarData CarData => _carData;
        public float StoppingDragSpeedThreshold => _stoppingDragSpeedThreshold;
        public bool UseBrakeToGoBack { get => _useBrakeToGoBack; set => _useBrakeToGoBack = value; }
        public CarState CarState => _carState;
        public Rigidbody Rigidbody => _rigidbody;
        #endregion

        public void RegisterModifier(IMotorTorqueModifier motorTorqueModifier)
        {
            if (motorTorqueModifier == null) return;

            if (!_motorTorqueModifiers.Contains(motorTorqueModifier))
            {
                _motorTorqueModifiers.Add(motorTorqueModifier);
            }
        }

        public void UnregisterModifier(IMotorTorqueModifier motorTorqueModifier)
        {
            if (motorTorqueModifier == null) return;

            if (_motorTorqueModifiers.Contains(motorTorqueModifier))
            {
                _motorTorqueModifiers.Remove(motorTorqueModifier);
            }
        }

        public void SetAngularVelocity(Vector3 velocity)
        {
            _rigidbody.angularVelocity = velocity;
        }

        /// <summary>
        /// Resets the linear velocity of the object in world space.
        /// Try not to use this method frequently as it overrides physics calculations.
        /// </summary>
        /// <remarks>This method immediately updates the object's velocity. Any existing forces or
        /// momentum are overridden by the new value.</remarks>
        public void ResetLinearVelocity()
        {
            _rigidbody.linearVelocity = Vector3.zero;
        }

        public void ApplyForce(Vector3 forceAmount, ForceMode forceMode)
        {
            _rigidbody.AddForce(forceAmount, forceMode);
        }

        public void ApplyTorque(Vector3 torqueAmount, ForceMode forceMode)
        {
            _rigidbody.AddTorque(torqueAmount, forceMode);
        }

        public void SetInputProvider(ICarInputProvider carInputProvider)
        {
            if (carInputProvider != null)
            {
                _carInputProvider = carInputProvider;
            }
            else
            {
                LogX.LogError("Car Input Provider is null!");
            }
        }

        public void SetTargetSpeed(float targetSpeed)
        {
            _targetSpeed = targetSpeed;
        }

        /// <summary>
        /// To be completely airborne, the car must not be touching the ground, roof, or any sides.
        /// </summary>
        /// <returns></returns>
        public bool AreAllWheelsAirborne()
        {
            return !_fLWheel.WheelState.IsGrounded &&
                   !_fRWheel.WheelState.IsGrounded &&
                   !_rLWheel.WheelState.IsGrounded &&
                   !_rRWheel.WheelState.IsGrounded;
        }

        public bool AreAllWheelsGrounded()
        {
            return _fLWheel.WheelState.IsGrounded &&
                   _fRWheel.WheelState.IsGrounded &&
                   _rLWheel.WheelState.IsGrounded &&
                   _rRWheel.WheelState.IsGrounded;
        }

        #region Unity Methods

        private void OnDrawGizmos()
        {
            if (_drawWheelRays
                && _fLWheel != null
                && _fRWheel != null
                && _rLWheel != null
                && _rRWheel != null)
            {
                if (_fLWheel.WheelState.IsGrounded)
                {
                    UnityEngine.Debug.DrawRay(
                        _fLWheel.WheelPoint.position,
                        -_fLWheel.WheelPoint.transform.up * _suspensionDistance,
                        Color.red);
                }
                else
                {
                    UnityEngine.Debug.DrawRay(
                        _fLWheel.WheelPoint.position,
                        -_fLWheel.WheelPoint.transform.up * _suspensionDistance,
                        Color.green);
                }

                if (_fRWheel.WheelState.IsGrounded)
                {
                    UnityEngine.Debug.DrawRay(
                        _fRWheel.WheelPoint.position,
                        -_fRWheel.WheelPoint.transform.up * _suspensionDistance,
                        Color.red);
                }
                else
                {
                    UnityEngine.Debug.DrawRay(
                        _fRWheel.WheelPoint.position,
                        -_fRWheel.WheelPoint.transform.up * _suspensionDistance,
                        Color.green);
                }

                if (_rLWheel.WheelState.IsGrounded)
                {
                    UnityEngine.Debug.DrawRay(
                        _rLWheel.WheelPoint.position,
                        -_rLWheel.WheelPoint.transform.up * _suspensionDistance,
                        Color.red);
                }
                else
                {
                    UnityEngine.Debug.DrawRay(
                        _rLWheel.WheelPoint.position,
                        -_rLWheel.WheelPoint.transform.up * _suspensionDistance,
                        Color.green);
                }

                if (_rRWheel.WheelState.IsGrounded)
                {
                    UnityEngine.Debug.DrawRay(
                        _rRWheel.WheelPoint.position,
                        -_rRWheel.WheelPoint.transform.up * _suspensionDistance,
                        Color.red);
                }
                else
                {
                    UnityEngine.Debug.DrawRay(
                        _rRWheel.WheelPoint.position,
                        -_rRWheel.WheelPoint.transform.up * _suspensionDistance,
                        Color.green);
                }
            }

            if (_drawCOM && _comTransform != null)
            {
                Gizmos.color = _drawCOMColor;
                Gizmos.DrawSphere(_comTransform.position, _drawCOMRadius);
            }

            // Draw Ray Cheks
            if (_debugDrawEnvRayCheck)
            {
                // Roof Check
                UnityEngine.Debug.DrawRay(
                    transform.position,
                    transform.up * _roofCheckDistance,
                    Color.green);

                // Ground Check
                UnityEngine.Debug.DrawRay(
                    transform.position,
                    -transform.up * _groundCheckDistance,
                    Color.green);

                // Left Check
                UnityEngine.Debug.DrawRay(
                    transform.position,
                    -transform.right * _leftCheckDistance,
                    Color.green);

                // Right Check
                UnityEngine.Debug.DrawRay(
                    transform.position,
                    transform.right * _rightCheckDistance,
                    Color.green);

                // Front Check
                UnityEngine.Debug.DrawRay(
                    transform.position,
                    transform.forward * _frontCheckDistance,
                    Color.green);

                // Back Check
                UnityEngine.Debug.DrawRay(
                    transform.position,
                    -transform.forward * _backCheckDistance,
                    Color.green);

            }

            if (_drawWheelSphere)
            {
                Gizmos.color = _wheelSphereColor;
                Gizmos.DrawWireSphere(_fLWheel.WheelPoint.position, _wheelSphereRadius);
                Gizmos.DrawWireSphere(_fRWheel.WheelPoint.position, _wheelSphereRadius);
                Gizmos.DrawWireSphere(_rLWheel.WheelPoint.position, _wheelSphereRadius);
                Gizmos.DrawWireSphere(_rRWheel.WheelPoint.position, _wheelSphereRadius);
            }

            if (_debugDrawCarAxes)
            {
                if (_fLWheel == null
                    || _fRWheel == null
                    || _rLWheel == null
                    || _rRWheel == null) return;
                Gizmos.color = _debugCarAxesColor;

                // Draw front and rear axle
                Gizmos.DrawLine(
                    _fLWheel.WheelPoint.position,
                    _fRWheel.WheelPoint.position);
                Gizmos.DrawLine(
                    _rLWheel.WheelPoint.position,
                    _rRWheel.WheelPoint.position);

                // Draw wheel base
                Vector3 middlePointOfFrontAxle = (_fLWheel.WheelPoint.position + _fRWheel.WheelPoint.position) / 2f;
                Vector3 middlePointOfRearAxle = (_rLWheel.WheelPoint.position + _rRWheel.WheelPoint.position) / 2f;
                Gizmos.DrawLine(
                    middlePointOfFrontAxle,
                    middlePointOfRearAxle);
            }

            if (_drawCarVelocity)
            {
                if (_rigidbody == null) return;
                UnityEngine.Debug.DrawRay(transform.position, _rigidbody.linearVelocity, Color.green);
            }
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_initByCarData)
            {
                InitByCarData();
            }
        }

        private void InitByCarData()
        {
            // Acceleration
            _topSpeed = _carData.TopSpeed;
            _reverseTopSpeed = _carData.ReverseTopSpeed;
            _useMotorTorqueCurve = _carData.UseMotorTorqueCurve;
            _torqueCurve = _carData.TorqueCurve;
            _drivetrain = _carData.Drivetrain;
            _motorTorque = _carData.MotorTorque;
            _brakeForce = _carData.BrakeForce;
            _handBrakeForce = _carData.HandBrakeForce;
            _brakeBias = _carData.BrakeBias;
            _forwardFriction = _carData.ForwardFriction;
            _normalDrag = _carData.NormalDrag;
            _stoppingDrag = _carData.StoppingDrag;

            // Steering
            _fGripStrength = _carData.FGripStrength;
            _rGripStrength = _carData.RGripStrength;
            _steeringAngle = _carData.SteeringAngle;
            _useSteerAngleCurve = _carData.UseSteerAngleCurve;
            _steeringAngleSpeedCurve = _carData.SteeringAngleSpeedCurve;
            _steerSpeed = _carData.SteerSpeed;

            // Suspension
            _suspensionStrength = _carData.SuspensionStrength;
            _suspensionDamping = _carData.SuspensionDamping;
            _suspensionDistance = _carData.SuspensionDistance;
            _suspensionCheckLayer = _carData.SuspensionCheckLayer;

            // Wheel Mesh
            _wheelRadius = _carData.WheelRadius;
            _wheelSyncPositionSpeed = _carData.WheelSyncPositionSpeed;
            _wheelOffset = _carData.WheelOffset;
            _wheelRollingSpeed = _carData.WheelRollingSpeed;

            // Drift
            _driftActivationSpeed = _carData.DriftActivationSpeed;
            _minSlipAngleForDrift = _carData.MinSlipAngleForDrift;
            _maxSlipAngleForDrift = _carData.MaxSlipAngleForDrift;
            _frontWheelDriftGrip = _carData.FrontWheelDriftGrip;
            _backWheelDriftGrip = _carData.BackWheelDriftGrip;
            _driftKickTorque = _carData.DriftKickTorque;

            // Environment Check
            _isEnvCheckEnabled = _carData.IsEnvCheckEnabled;
            _envCheckLayer = _carData.EnvCheckLayer;
            _groundCheckDistance = _carData.GroundCheckDistance;
            _roofCheckDistance = _carData.RoofCheckDistance;
            _leftCheckDistance = _carData.LeftCheckDistance;
            _rightCheckDistance = _carData.RightCheckDistance;
            _frontCheckDistance = _carData.FrontCheckDistance;
            _backCheckDistance = _carData.BackCheckDistance;
        }

        private void Start()
        {
            _carInputProvider = GetComponent<ICarInputProvider>();
            if (_carInputProvider == null)
            {
                LogX.LogWarning("No ICarInputProvider found on the car.");

                return;
            }

            OnEngineStarted?.Invoke();
        }

        private void FixedUpdate()
        {
            _rigidbody.centerOfMass = _comTransform.localPosition;

            //!IMPORTANT: wheel state of wheels must be updated before applying any forces
            _fLWheel.WheelState.IsGrounded = CheckSuspension(
                _fLWheel.WheelPoint,
                out RaycastHit fLHit);
            _fLWheel.WheelState.Hit = fLHit;

            _fRWheel.WheelState.IsGrounded = CheckSuspension(
                _fRWheel.WheelPoint,
                out RaycastHit fRHit);
            _fRWheel.WheelState.Hit = fRHit;

            _rLWheel.WheelState.IsGrounded = CheckSuspension(
                _rLWheel.WheelPoint,
                out RaycastHit rLHit);
            _rLWheel.WheelState.Hit = rLHit;

            _rRWheel.WheelState.IsGrounded = CheckSuspension(
                _rRWheel.WheelPoint,
                out RaycastHit rRHit);
            _rRWheel.WheelState.Hit = rRHit;

            DetermineCarTorqueDirection();
            HandleDriftLogic();

            ApplyIdleDrag();

            DrawWheelAxes(_fLWheel.WheelPoint);
            DrawWheelAxes(_fRWheel.WheelPoint);
            DrawWheelAxes(_rLWheel.WheelPoint);
            DrawWheelAxes(_rRWheel.WheelPoint);

            Steer();

            ApplyForwardFriction(_fLWheel);
            ApplyForwardFriction(_fRWheel);
            ApplyForwardFriction(_rLWheel);
            ApplyForwardFriction(_rRWheel);

            ApplySuspensionForce(_fLWheel);
            ApplySuspensionForce(_fRWheel);
            ApplySuspensionForce(_rLWheel);
            ApplySuspensionForce(_rRWheel);

            ApplySidewaysFriction(_fLWheel, isFrontWheel: true);
            ApplySidewaysFriction(_fRWheel, isFrontWheel: true);
            ApplySidewaysFriction(_rLWheel, isFrontWheel: false);
            ApplySidewaysFriction(_rRWheel, isFrontWheel: false);

            switch (_drivetrain)
            {
                case Drivetrain.FrontWheelDrive:
                    ApplyMotorTorque(_fLWheel);
                    ApplyMotorTorque(_fRWheel);
                    break;
                case Drivetrain.RearWheelDrive:
                    ApplyMotorTorque(_rLWheel);
                    ApplyMotorTorque(_rRWheel);
                    break;
                case Drivetrain.AllWheelDrive:
                    ApplyMotorTorque(_fLWheel);
                    ApplyMotorTorque(_fRWheel);
                    ApplyMotorTorque(_rLWheel);
                    ApplyMotorTorque(_rRWheel);
                    break;
            }

            ApplyBrakeForce(_fLWheel, isFrontWheel: true);
            ApplyBrakeForce(_fRWheel, isFrontWheel: true);
            ApplyBrakeForce(_rLWheel, isFrontWheel: false);
            ApplyBrakeForce(_rRWheel, isFrontWheel: false);

            ApplyHandBrakeForce(_fLWheel, isFrontWheel: true);
            ApplyHandBrakeForce(_fRWheel, isFrontWheel: true);
            ApplyHandBrakeForce(_rLWheel, isFrontWheel: false);
            ApplyHandBrakeForce(_rRWheel, isFrontWheel: false);

            SyncWheelMeshes(_fLWheel, isFrontAxle: true, isLeftWheel: true);
            SyncWheelMeshes(_fRWheel, isFrontAxle: true, isLeftWheel: false);
            SyncWheelMeshes(_rLWheel, isFrontAxle: false, isLeftWheel: true);
            SyncWheelMeshes(_rRWheel, isFrontAxle: false, isLeftWheel: false);

            UpdateCarState();

            //!IMPORTANT: This must be the last line of code in FixedUpdate
            _previousSpeed = CarUtils.GetSpeed(_rigidbody);
        }

        private void UpdateCarState()
        {
            CarEnvCheckResultContainer carEnvCheckResultContainer = default;
            if (_isEnvCheckEnabled)
            {
                // left check
                bool isLeftHit = Physics.Raycast(
                    transform.position,
                    -transform.right,
                    _leftCheckDistance,
                    _envCheckLayer);

                // right check
                bool isRightHit = Physics.Raycast(
                    transform.position,
                    transform.right,
                    _rightCheckDistance,
                    _envCheckLayer);

                // up check
                bool isUpHit = Physics.Raycast(
                    transform.position,
                    transform.up,
                    _roofCheckDistance,
                    _envCheckLayer);

                // down check
                bool isDownHit = Physics.Raycast(
                    transform.position,
                    -transform.up,
                    _groundCheckDistance,
                    _envCheckLayer);

                // front check
                bool isFrontHit = Physics.Raycast(
                    transform.position,
                    transform.forward,
                    _frontCheckDistance,
                    _envCheckLayer);

                // back check
                bool isBackHit = Physics.Raycast(
                    transform.position,
                    -transform.forward,
                    _backCheckDistance,
                    _envCheckLayer);

                // construct result
                CarEnvCheckResult leftCheckResult = new(default, isLeftHit);
                CarEnvCheckResult rightCheckResult = new(default, isRightHit);
                CarEnvCheckResult upCheckResult = new(default, isUpHit);
                CarEnvCheckResult downCheckResult = new(default, isDownHit);
                CarEnvCheckResult frontCheckResult = new(default, isFrontHit);
                CarEnvCheckResult backCheckResult = new(default, isBackHit);

                carEnvCheckResultContainer =
                    new(leftCheckResult, rightCheckResult, frontCheckResult, backCheckResult, upCheckResult, downCheckResult);
            }


            _carState = new(
                speed: CarUtils.GetSpeed(_rigidbody),
                speedNormalized: CarUtils.GetSpeedNormalized(_rigidbody, _topSpeed),
                rbSpeed: CarUtils.GetRbSpeed(_rigidbody),
                angularVelocity: _rigidbody.angularVelocity,
                slipDirection: CarUtils.GetSlipDirection(_rigidbody),
                slipAngle: CarUtils.GetSlipAngle(transform, _rigidbody),
                availableTorque: CarUtils.GetAvailableTorque(_rigidbody, _motorTorque, _topSpeed, _torqueCurve),
                acceleration: CarUtils.GetAcceleration(_rigidbody, _previousSpeed),
                isDrifting: _isDrifting,
                carEnvCheckResultContainer: carEnvCheckResultContainer);
        }

        #endregion

        private float GetDesiredSpeed()
        {
            return _targetSpeed <= CarConstants.MinForwardSpeed ? _topSpeed : _targetSpeed;
        }

        /// <summary>
        /// Apply drag to the car when it's not moving and no gas or brake input is pressed
        /// </summary>
        private void ApplyIdleDrag()
        {
            if (CarUtils.GetSpeedAbs(_rigidbody) < StoppingDragSpeedThreshold
                && _carInputProvider.FrameInput.Gas == 0f
                && _carInputProvider.FrameInput.Brake == 0f
                && AreAllWheelsGrounded())
            {
                _rigidbody.linearDamping = _stoppingDrag;
            }
            else
            {
                _rigidbody.linearDamping = _normalDrag;
            }
        }

        private void SyncWheelMeshes(Wheel wheel, bool isFrontAxle, bool isLeftWheel)
        {
            Vector3 targetPosition;
            if (wheel.WheelState.IsGrounded)
            {
                targetPosition =
                    wheel.WheelPoint.position
                    + (-wheel.WheelPoint.up * wheel.WheelState.Hit.distance) + (wheel.WheelPoint.up * _wheelRadius);
            }
            else
            {
                targetPosition =
                    wheel.WheelPoint.position
                    + (-wheel.WheelPoint.up * _suspensionDistance) + (wheel.WheelPoint.up * _wheelRadius);
            }

            // Apply offset
            Vector3 wheelOffsetYVector = _wheelOffset.y * wheel.WheelPoint.up;
            Vector3 whellOffsetXVector = isLeftWheel ? -_wheelOffset.x * wheel.WheelPoint.right : _wheelOffset.x * wheel.WheelPoint.right;
            Vector3 wheelOffsetZVector = isFrontAxle ? _wheelOffset.z * wheel.WheelPoint.forward : -_wheelOffset.z * wheel.WheelPoint.forward;

            targetPosition += wheelOffsetYVector + whellOffsetXVector + wheelOffsetZVector;

            wheel.WheelMesh.position = Vector3.Lerp(
                wheel.WheelMesh.position,
                targetPosition,
                _wheelSyncPositionSpeed * Time.fixedDeltaTime);

            wheel.WheelMesh.position = targetPosition;

            // Calculate rolling angle
            bool isDrivenAxle =
                (isFrontAxle && (_drivetrain == Drivetrain.FrontWheelDrive || _drivetrain == Drivetrain.AllWheelDrive)) ||
                (!isFrontAxle && (_drivetrain == Drivetrain.RearWheelDrive || _drivetrain == Drivetrain.AllWheelDrive));

            if (wheel.WheelState.IsGrounded || (!wheel.WheelState.IsGrounded && isDrivenAxle))
            {
                float wheelCircumference = 2 * Mathf.PI * _wheelRadius;
                float wheelSpeed = Vector3.Dot(_rigidbody.linearVelocity, wheel.WheelPoint.forward);
                float rollingDelta = (wheelSpeed / wheelCircumference) * 360f * Time.fixedDeltaTime * _wheelRollingSpeed;
                wheel.WheelState.RollingAngle += rollingDelta;
                wheel.WheelState.RollingAngle %= 360f;
            }

            // Apply rolling (X-axis) and steering (Y-axis)
            Quaternion rollRotation = Quaternion.Euler(wheel.WheelState.RollingAngle, 0f, 0f);
            Quaternion steeringRotation = Quaternion.Euler(0f, wheel.WheelPoint.localEulerAngles.y, 0f);
            wheel.WheelMesh.localRotation = steeringRotation * rollRotation;
        }

        private void Steer()
        {
            // Get steer speed based on car speed
            float targetSteerSpeed;
            targetSteerSpeed = _steerSpeed;

            // Get target steer angle based on input
            float targetSteerAngle;
            if (_useSteerAngleCurve)
            {
                targetSteerAngle =
                    _carInputProvider.FrameInput.Steer
                    * _steeringAngle
                    * _steeringAngleSpeedCurve.Evaluate(
                        CarUtils.GetSpeedNormalized(_rigidbody, _topSpeed));
            }
            else
            {
                targetSteerAngle = _carInputProvider.FrameInput.Steer * _steeringAngle;
            }

            _currentSteerAngle = Mathf.Lerp(_currentSteerAngle, targetSteerAngle, Time.fixedDeltaTime * targetSteerSpeed);

            _fLWheel.WheelPoint.localEulerAngles = new Vector3(0, _currentSteerAngle, 0);
            _fRWheel.WheelPoint.localEulerAngles = new Vector3(0, _currentSteerAngle, 0);
        }

        private void DrawWheelAxes(Transform wheelTransform)
        {
            if (!_drawWheelAxes) return;

            UnityEngine.Debug.DrawRay(wheelTransform.position, wheelTransform.forward * _wheelAxesLength, Color.blue);
            UnityEngine.Debug.DrawRay(wheelTransform.position, wheelTransform.up * _wheelAxesLength, Color.green);
            UnityEngine.Debug.DrawRay(wheelTransform.position, wheelTransform.right * _wheelAxesLength, Color.red);
        }

        private bool CheckSuspension(Transform wheelTransform, out RaycastHit hit)
        {
            Ray suspensionRay = new(wheelTransform.position, -transform.up);

            return Physics.Raycast(suspensionRay, out hit, _suspensionDistance, _suspensionCheckLayer);
        }

        /// <summary>
        /// Apply forward friction to the wheels when no gas or brake input is pressed
        /// to stop the car from moving forward or backward
        /// </summary>
        /// <param name="wheelTransform"></param>
        private void ApplyForwardFriction(Wheel wheel)
        {
            if (_carInputProvider.FrameInput.Gas != 0f || _carInputProvider.FrameInput.Brake != 0) return;
            if (!AreAllWheelsGrounded()) return;

            Vector3 frictionDirection = IsCarMovingForward() ? -wheel.WheelPoint.forward : wheel.WheelPoint.forward;
            float carSpeed = Vector3.Dot(_rigidbody.transform.forward, _rigidbody.linearVelocity);

            if (Mathf.Abs(carSpeed) > 0f)
            {
                _rigidbody.AddForceAtPosition(_forwardFriction * frictionDirection, wheel.WheelPoint.position);
            }
        }

        private void ApplyBrakeForce(Wheel wheel, bool isFrontWheel)
        {
            if (!wheel.WheelState.IsGrounded) return;

            Vector3 brakeDirection = -wheel.WheelPoint.forward;

            if (_torqueDirection == TorqueDirection.Forward && _carInputProvider.FrameInput.Brake > 0f && IsCarMovingForward())
            {
                float frontWheelBrakeForce = _brakeForce * _brakeBias;
                float rearWheelBrakeForce = _brakeForce * (1 - _brakeBias);

                float brakeForce = isFrontWheel ? frontWheelBrakeForce : rearWheelBrakeForce;

                _rigidbody.AddForceAtPosition(brakeForce * _carInputProvider.FrameInput.Brake * brakeDirection, wheel.WheelPoint.position);
            }
        }

        private void ApplyHandBrakeForce(Wheel wheel, bool isFrontWheel)
        {
            if (!wheel.WheelState.IsGrounded) return;

            Vector3 brakeDirection = -wheel.WheelPoint.forward;

            if (_torqueDirection == TorqueDirection.Forward && _carInputProvider.FrameInput.Handbrake > 0f && IsCarMovingForward())
            {
                float frontWheelBrakeForce = _handBrakeForce * _brakeBias;
                float rearWheelBrakeForce = _handBrakeForce * (1 - _brakeBias);

                float handbrakeForce = isFrontWheel ? frontWheelBrakeForce : rearWheelBrakeForce;

                _rigidbody.AddForceAtPosition(handbrakeForce * _carInputProvider.FrameInput.Handbrake * brakeDirection, wheel.WheelPoint.position);
            }
        }

        private void ApplyMotorTorque(Wheel wheel)
        {
            if (!wheel.WheelState.IsGrounded) return;

            // Decide the direction of the torque based on torque direction
            Vector3 accelerationDirection = _torqueDirection switch
            {
                TorqueDirection.Forward => wheel.WheelPoint.forward,
                TorqueDirection.Backward => -wheel.WheelPoint.forward,
                _ => throw new ArgumentOutOfRangeException()
            };

            bool canApplyTorque = _torqueDirection == TorqueDirection.Forward && _carInputProvider.FrameInput.Gas > 0f ||
                                  _torqueDirection == TorqueDirection.Backward && _carInputProvider.FrameInput.Brake > 0f;
            if (!canApplyTorque) return;

            float availableTorque = EvaluateMotorTorque();

            // Apply motor torque modifiers
            float totalTorqueModifier = 0f;
            foreach (var modifier in _motorTorqueModifiers)
            {
                totalTorqueModifier += modifier.Modifier;
            }

            availableTorque += totalTorqueModifier;

            // Divide the available torque by number of driven wheels
            int drivenWheelsCount = _drivetrain switch
            {
                Drivetrain.FrontWheelDrive => 2,
                Drivetrain.RearWheelDrive => 2,
                Drivetrain.AllWheelDrive => 4,
                _ => 1
            };

            availableTorque /= drivenWheelsCount;

            // Throttle input is dependent on torque direction
            float throttleInput = _torqueDirection == TorqueDirection.Forward
                ? _carInputProvider.FrameInput.Gas
                : _carInputProvider.FrameInput.Brake;

            _rigidbody.AddForceAtPosition(
                throttleInput * availableTorque * accelerationDirection,
                wheel.WheelPoint.position);
        }

        private float EvaluateMotorTorque()
        {
            float carSpeedAbs = CarUtils.GetSpeedAbs(_rigidbody);
            float forwardSpeedLimit = GetDesiredSpeed();
            float topSpeedBasedOnTorqueDirection =
                _torqueDirection == TorqueDirection.Forward
                ? forwardSpeedLimit
                : _reverseTopSpeed;
            if (carSpeedAbs > topSpeedBasedOnTorqueDirection)
            {
                return 0f;
            }

            // if motor torque curve is not used, just take plain motor torque 
            float availableTorque = _motorTorque;
            if (_useMotorTorqueCurve)
            {
                float normalizedSpeed = Mathf.Clamp01(carSpeedAbs / _topSpeed);
                availableTorque = _torqueDirection switch
                {
                    TorqueDirection.Forward => _torqueCurve.Evaluate(normalizedSpeed) * _motorTorque,
                    TorqueDirection.Backward => _motorTorque,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return availableTorque;
        }

        private void ApplySuspensionForce(Wheel wheel)
        {
            if (wheel.WheelState.IsGrounded)
            {
                Vector3 springDirection = wheel.WheelPoint.up;

                Vector3 tireWorldVelocity = _rigidbody.GetPointVelocity(wheel.WheelPoint.position);

                float offset = _suspensionDistance - wheel.WheelState.Hit.distance;

                float springVelocity = Vector3.Dot(springDirection, tireWorldVelocity);

                float springForce = (offset * _suspensionStrength) - (springVelocity * _suspensionDamping);

                _rigidbody.AddForceAtPosition(springForce * springDirection, wheel.WheelPoint.position);
            }
        }

        private void ApplySidewaysFriction(Wheel wheel, bool isFrontWheel)
        {
            if (wheel.WheelState.IsGrounded)
            {
                Vector3 steeringDirection = wheel.WheelPoint.right;

                Vector3 tireWorldVelocity = _rigidbody.GetPointVelocity(wheel.WheelPoint.position);

                float steeringVelocity = Vector3.Dot(steeringDirection, tireWorldVelocity);

                float gripFactor;
                if (isFrontWheel)
                {
                    if (_isDrifting)
                    {
                        gripFactor = _frontWheelDriftGrip;
                    }
                    else
                    {
                        gripFactor = _useGripCurves
                            ? _fGripCurve.Evaluate(CarUtils.GetSpeedNormalized(_rigidbody, _topSpeed))
                            : _fGripFactor;
                    }
                }
                else
                {
                    if (_isDrifting)
                    {
                        gripFactor = _backWheelDriftGrip;
                    }
                    else
                    {
                        gripFactor = _useGripCurves
                            ? _rGripCurve.Evaluate(CarUtils.GetSpeedNormalized(_rigidbody, _topSpeed))
                            : _rGripFactor;
                    }
                }

                float desiredSteeringVelocityChange = -steeringVelocity * gripFactor;

                float desiredAcceleration = desiredSteeringVelocityChange / Time.fixedDeltaTime;

                float gripStrength = isFrontWheel ? _fGripStrength : _rGripStrength;

                _rigidbody.AddForceAtPosition(gripStrength * desiredAcceleration * steeringDirection, wheel.WheelPoint.position);
            }

        }

        private void HandleDriftLogic()
        {
            float driftAngle = Mathf.Abs(CarUtils.GetSlipAngle(transform, _rigidbody));

            // Drift Start Condition
            if (!_isDrifting
                && _carInputProvider.FrameInput.Handbrake > 0f
                && CarUtils.GetSpeed(_rigidbody) > _driftActivationSpeed
                && AreAllWheelsGrounded())
            {
                _isDrifting = true;
                OnDriftStarted?.Invoke();

                // Artificial yaw kick
                float torqueDirection = _carInputProvider.FrameInput.Steer >= 0 ? 1f : -1f;
                _rigidbody.AddTorque(_driftKickTorque * torqueDirection * Vector3.up, ForceMode.Impulse);
            }
            // Drift End Condition
            else if (_isDrifting
                && (driftAngle < _minSlipAngleForDrift || driftAngle > _maxSlipAngleForDrift || !AreAllWheelsGrounded())) // Drift End Condition
            {
                _isDrifting = false;
                OnDriftEnded?.Invoke();
            }

            if (_isDrifting && _carInputProvider.FrameInput.Handbrake > 0f)
            {
                // Artificial yaw kick
                float torqueDirection = CarUtils.GetSlipDirection(_rigidbody) * -1f;
                _rigidbody.AddTorque(_driftKickTorque * torqueDirection * Vector3.up, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// Determine the direction of the torque based on the car speed and brake input
        /// </summary>
        private void DetermineCarTorqueDirection()
        {
            float carSpeed = CarUtils.GetSpeed(_rigidbody);

            // If car is about the stop and brake input is pressed then apply torque in opposite direction
            if (carSpeed < CarConstants.MinForwardSpeed
                && _carInputProvider.FrameInput.Brake > 0f
                && UseBrakeToGoBack)
            {
                _torqueDirection = TorqueDirection.Backward;
            }
            else
            {
                _torqueDirection = TorqueDirection.Forward;
            }
        }

        private bool IsCarMovingForward()
        {
            return CarUtils.GetSpeed(_rigidbody) > CarConstants.MinForwardSpeed;
        }
    }
}
