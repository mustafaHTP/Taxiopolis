using Framework.FSM;
using Game.Systems.EmoteSystem;
using Game.Systems.PassengerSystem.Passengers.FSM;
using Game.Utils;
using UnityEngine;

namespace Game.Systems.PassengerSystem.Passengers
{
    public class Passenger : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _debugDrawGizmos;
        [SerializeField] private Color _debugChaseDistanceColor = Color.yellow;
        [SerializeField] private Color _debugPickedUpDistanceColor = Color.green;
        [SerializeField] private Color _debugTargetLineColor = Color.cyan;

        [Header("Passenger Settings")]
        [SerializeField] PassengerWaitingPoint _waitingPoint;
        [SerializeField] PassengerDestinationPoint _destinationPoint;
        [SerializeField] private float _pickedUpDistance = 1.5f;
        [SerializeField] private float _chaseDistance = 10f;
        [SerializeField] private float _baseTripTime;
        [SerializeField] private float _tripDistance;

        [Header("References")]
        [SerializeField] private GameObject _meshesGameObject;
        [SerializeField] private EmoteController _emoteController;

        public PassengerWaitingPoint WaitingPoint => _waitingPoint;
        public PassengerDestinationPoint DestinationPoint => _destinationPoint;
        public EmoteController EmoteController => _emoteController;
        public GameObject MeshesGameObject => _meshesGameObject;
        public CarPassengerHandler NotifyingCar { get; private set; }
        public Collider Collider { get; private set; }
        public PassengerMovementHandler PassengerMovementHandler { get; private set; }
        public PassengerAnimationHandler PassengerAnimationHandler { get; private set; }
        public PassengerJob Job { get; private set; }

        private bool _isCarAvailableToGetIn = false;
        private bool _canDroppedOff = false;
        private float _distanceToNotifyingCarSqr = float.PositiveInfinity;

        public IState CurrentState => _stateMachine?.Current;
        private IState _initialState;
        private StateMachine _stateMachine;

        public void AssignJob(PassengerJob passengerJob)
        {
            Job = passengerJob;

            _waitingPoint = Job.WaitingPoint;
            _destinationPoint = Job.DestinationPoint;
        }

        public void ResetState()
        {
            _stateMachine?.ChangeState(_initialState);
            ResetNotificationState();
            Collider.enabled = true;
            _canDroppedOff = false;
        }

        public void ResetNotificationState()
        {
            NotifyingCar = null;
        }

        public void DropOff()
        {
            _canDroppedOff = true;
        }

        public void NotifyCarNearby(CarPassengerHandler car)
        {
            if (NotifyingCar != null) return;

            if (_waitingPoint == null || _destinationPoint == null)
            {
                LogX.LogWarning("Passenger waiting point or destination point is not set.");

                return;
            }

            NotifyingCar = car;
        }

        public Transform GetClosestEntryPointOfCar()
        {
            if (NotifyingCar == null)
            {
                LogX.LogWarning("No notifying car to get entry points from.");

                return null;
            }

            if (NotifyingCar.CarEntryPoints == null || NotifyingCar.CarEntryPoints.Length == 0)
            {
                LogX.LogWarning("Notifying car has no entry points defined.");

                return null;
            }

            Transform closestEntryPoint = NotifyingCar.CarEntryPoints[0];
            float closestDistance = Vector3.Distance(
                transform.position,
                 NotifyingCar.CarEntryPoints[0].position);

            for (int i = 1; i < NotifyingCar.CarEntryPoints.Length; i++)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    NotifyingCar.CarEntryPoints[i].position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEntryPoint = NotifyingCar.CarEntryPoints[i];
                }
            }

            return closestEntryPoint;
        }

        private void OnDrawGizmos()
        {
            if (!_debugDrawGizmos) return;

            if (NotifyingCar != null)
            {
                Gizmos.color = _debugTargetLineColor;
                Gizmos.DrawLine(transform.position, NotifyingCar.transform.position);
            }

            Gizmos.color = _debugPickedUpDistanceColor;
            Gizmos.DrawWireSphere(transform.position, _pickedUpDistance);

            Gizmos.color = _debugChaseDistanceColor;
            Gizmos.DrawWireSphere(transform.position, _chaseDistance);

            if (WaitingPoint != null && DestinationPoint != null)
            {
                Gizmos.color = _debugTargetLineColor;
                Gizmos.DrawLine(WaitingPoint.transform.position, DestinationPoint.transform.position);
            }
        }

        private void Awake()
        {
            PassengerMovementHandler = GetComponent<PassengerMovementHandler>();
            PassengerAnimationHandler = GetComponent<PassengerAnimationHandler>();
            Collider = GetComponent<Collider>();

            _stateMachine = new StateMachine();

            var waitingState = new WaitingState(this, "Waiting");
            var chaseUpState = new ChaseUpState(this, "ChaseUp");
            var droppedOffState = new DroppedOffState(this, "DroppedOff");
            var pickedUpState = new PickedUpState(this, "PickedUp");
            var angryState = new AngryState(this, "Angry");

            _initialState = waitingState;

            _stateMachine.AddAnyTransition(
                angryState,
                new FuncPredicate(() => _canDroppedOff && Job.State == JobState.Failed));
            _stateMachine.AddTransition(
                waitingState,
                chaseUpState,
                new FuncPredicate(() => _isCarAvailableToGetIn && _distanceToNotifyingCarSqr <= _chaseDistance * _chaseDistance));
            _stateMachine.AddTransition(
                chaseUpState,
                pickedUpState,
                new FuncPredicate(() => _isCarAvailableToGetIn && _distanceToNotifyingCarSqr <= _pickedUpDistance * _pickedUpDistance));
            _stateMachine.AddTransition(
                chaseUpState,
                waitingState,
               new FuncPredicate(() => !_isCarAvailableToGetIn || _distanceToNotifyingCarSqr > _chaseDistance * _chaseDistance));
            _stateMachine.AddTransition(
                pickedUpState,
                droppedOffState,
                new FuncPredicate(() => _canDroppedOff && Job.State == JobState.Completed));

            _stateMachine.SetState(waitingState);
        }

        private void Start()
        {
            _stateMachine.Init();
        }

        private void Update()
        {
            _isCarAvailableToGetIn = NotifyingCar != null && NotifyingCar.AnyAvailableSeat();
            _distanceToNotifyingCarSqr = NotifyingCar != null
            ? (transform.position - NotifyingCar.transform.position).sqrMagnitude
            : float.PositiveInfinity;

            _stateMachine?.Update();
        }
    }
}
