using UnityEngine;
using static Game.Car.CarEnums;

namespace Game.Car
{
    [CreateAssetMenu(fileName = "New Car Data", menuName = "Game/Car/Car Data")]
    public class CarData : ScriptableObject
    {
        [SerializeField] private string _name;

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

        public string Name => _name;
        public float TopSpeed => _topSpeed;
        public float ReverseTopSpeed => _reverseTopSpeed;
        public bool UseMotorTorqueCurve => _useMotorTorqueCurve;
        public AnimationCurve TorqueCurve => _torqueCurve;
        public Drivetrain Drivetrain => _drivetrain;
        public float MotorTorque => _motorTorque;
        public float BrakeForce => _brakeForce;
        public float HandBrakeForce => _handBrakeForce;
        public float BrakeBias => _brakeBias;
        public float ForwardFriction => _forwardFriction;
        public float NormalDrag => _normalDrag;
        public float StoppingDrag => _stoppingDrag;
        public float StoppingDragSpeedThreshold => _stoppingDragSpeedThreshold;
        public float WheelRadius => _wheelRadius;
        public float WheelSyncPositionSpeed => _wheelSyncPositionSpeed;
        public Vector3 WheelOffset => _wheelOffset;
        public float WheelRollingSpeed => _wheelRollingSpeed;

        // Suspension
        public float SuspensionStrength => _suspensionStrength;
        public float SuspensionDamping => _suspensionDamping;
        public float SuspensionDistance => _suspensionDistance;
        public LayerMask SuspensionCheckLayer => _suspensionCheckLayer;

        // Grip and steering
        public float FGripStrength => _fGripStrength;
        public float RGripStrength => _rGripStrength;
        public float SteeringAngle => _steeringAngle;
        public bool UseSteerAngleCurve => _useSteerAngleCurve;
        public AnimationCurve SteeringAngleSpeedCurve => _steeringAngleSpeedCurve;
        public float SteerSpeed => _steerSpeed;
        public bool UseGripCurves => _useGripCurves;
        public AnimationCurve FGripCurve => _fGripCurve;
        public AnimationCurve RGripCurve => _rGripCurve;
        public float FGripFactor => _fGripFactor;
        public float RGripFactor => _rGripFactor;

        // Drift
        public float DriftActivationSpeed => _driftActivationSpeed;
        public float MinSlipAngleForDrift => _minSlipAngleForDrift;
        public float MaxSlipAngleForDrift => _maxSlipAngleForDrift;
        public float FrontWheelDriftGrip => _frontWheelDriftGrip;
        public float BackWheelDriftGrip => _backWheelDriftGrip;
        public float DriftKickTorque => _driftKickTorque;

        // Env check
        public bool IsEnvCheckEnabled => _isEnvCheckEnabled;
        public LayerMask EnvCheckLayer => _envCheckLayer;
        public float GroundCheckDistance => _groundCheckDistance;
        public float RoofCheckDistance => _roofCheckDistance;
        public float LeftCheckDistance => _leftCheckDistance;
        public float RightCheckDistance => _rightCheckDistance;
        public float FrontCheckDistance => _frontCheckDistance;
        public float BackCheckDistance => _backCheckDistance;
    }
}
