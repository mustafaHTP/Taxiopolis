using Framework.Services;
using Game.Input;
using Game.Managers;
using Game.Systems.ComboSystem;
using Game.Systems.PassengerSystem.Passengers;
using Game.Systems.SoundSystem;
using System;
using UnityEngine;
using LogX = Game.Utils.LogX;

namespace Game.Systems.PassengerSystem
{
    public class CarPassengerHandler : MonoBehaviour
    {
        public event Action<PassengerJob, int> OnFareBonusEarned;
        public event Action<PassengerJob> OnPassengerPickedUp;
        public event Action<PassengerJob> OnPassengerDroppedOff;
        public event Action<PassengerJob> OnPassengerJobFinished;

        [Header("References")]
        [SerializeField] private SoundData _passengerPickUpAndDropOffSoundData;
        [SerializeField] private SoundData _passengerJobSuccessfulSoundData;
        [SerializeField] private GameObject _prefabPassengerDestinationAreaVfx;

        [Header("Settings")]
        [SerializeField] private Transform[] _carEntryPoints;
        [SerializeField] private int _maxNotifiablePassengerLimit = 3;
        [SerializeField]
        [Tooltip("Radius within which passengers will be notified when the horn is sounded.")]
        private float _passengerNotificationRadius = 1f;
        [SerializeField] private LayerMask _passengerLayerMask;

        [Header("Debug")]
        [SerializeField] private bool _debugPassengerNotificationRadius;
        [SerializeField] private Color _debugPassengerNotificationColor = Color.yellow;

        private Passenger _currentPassenger;
        private ICarInputProvider _carInputProvider;
        private GameObject _passengerDestinationAreaVfx;
        private CarComboHandler _carComboHandler;

        // Reused on every horn press so notifying passengers does not allocate.
        private Collider[] _notifyBuffer;

        public Transform[] CarEntryPoints => _carEntryPoints;

        // Null-safe: explicit check because ?. would bypass Unity's destroyed-object null check.
        public PassengerJob CurrentJob => _currentPassenger != null ? _currentPassenger.Job : null;

        public bool HasJob()
        {
            return _currentPassenger != null;
        }

        public bool AnyAvailableSeat()
        {
            return _currentPassenger == null;
        }

        public void GetIn(Passenger passenger)
        {
            if (!AnyAvailableSeat()) return;

            _currentPassenger = passenger;
            _currentPassenger.Job.Start(this);

            // The riding passenger leaves the passenger simulation: it is no longer counted
            // and can never be despawned while it is in the car.
            ServiceLocator
                .Scene
                .GetService<PassengerManager>()
                .NotifyPassengerPickedUp(passenger);

            ServiceLocator
                .Scene
                .GetService<GameTimeManager>()
                .AddGameTime(CurrentJob.PickupBonusTime);

            ServiceLocator
                .Global
                .GetService<SoundManager>()
                .PlaySound(_passengerPickUpAndDropOffSoundData);

            SpawnPassengerDestinationVfx();

            OnPassengerPickedUp?.Invoke(_currentPassenger.Job);
        }

        private void OnDrawGizmos()
        {
            if (_debugPassengerNotificationRadius)
            {
                Gizmos.color = _debugPassengerNotificationColor;
                Gizmos.DrawWireSphere(transform.position, _passengerNotificationRadius);
            }
        }

        private void Awake()
        {
            _carInputProvider = GetComponent<ICarInputProvider>();
            _carComboHandler = GetComponent<CarComboHandler>();

            _notifyBuffer = new Collider[Mathf.Max(1, _maxNotifiablePassengerLimit)];
        }

        private void Start()
        {
            _carInputProvider.HornAction.started += HornAction_started;
        }

        private void Update()
        {
            if (_currentPassenger != null)
            {
                _currentPassenger.Job.Tick(Time.deltaTime);
                // check job is failed
                if (_currentPassenger.Job.State == JobState.Failed)
                {
                    // before finishing job calculate job tier how successful player are
                    // i know it gets the worst result but i need to fill job result tier
                    // so i can play sound effects and show ui panel based on that 
                    PassengerJobResultTier passengerJobResultTier = ServiceLocator
                        .Scene
                        .GetService<PassengerManager>()
                        .PassengerJobHelper
                        .EvaluatePassengerJobResultTier(CurrentJob.TimeLeft, CurrentJob.TripTime);

                    CurrentJob.Complete(passengerJobResultTier);

                    ServiceLocator
                        .Global
                        .GetService<SoundManager>()
                        .PlaySound(passengerJobResultTier.VoiceOver);

                    OnPassengerJobFinished?.Invoke(CurrentJob);
                    DropOffPassenger();
                }
            }
        }

        private void OnEnable()
        {
            _carComboHandler.OnComboSuccess += CarComboHandler_OnComboSuccess;
        }

        private void OnDisable()
        {
            _carComboHandler.OnComboSuccess -= CarComboHandler_OnComboSuccess;
            _carInputProvider.HornAction.started -= HornAction_started;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_currentPassenger == null) return;
            // Check if the collider belongs to a PassengerPoint
            if (!other.TryGetComponent(out PassengerDestinationPoint passengerDestinationPoint)) return;

            // Attempt to drop off any passengers whose destination matches this destination point
            if (_currentPassenger.DestinationPoint == passengerDestinationPoint)
            {
                PassengerJob passengerJob = _currentPassenger.Job;
                // before finishing job calculate job tier how successful player are
                PassengerJobResultTier passengerJobResultTier = ServiceLocator
                    .Scene
                    .GetService<PassengerManager>()
                    .PassengerJobHelper
                    .EvaluatePassengerJobResultTier(passengerJob.TimeLeft, passengerJob.TripTime);

                // based on result, add game time
                ServiceLocator
                .Scene
                .GetService<GameTimeManager>()
                .AddGameTime(passengerJobResultTier.TimeBonus);

                // Finish job
                _currentPassenger.Job.Complete(passengerJobResultTier);

                DropOffPassenger();

                // Play sound effects
                ServiceLocator
                    .Global
                    .GetService<SoundManager>()
                    .PlaySound(_passengerJobSuccessfulSoundData);

                ServiceLocator
                .Global
                .GetService<SoundManager>()
                .PlaySound(passengerJobResultTier.VoiceOver);

                OnPassengerJobFinished?.Invoke(passengerJob);
            }
        }

        private void HornAction_started(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            NotifyNearbyPassengers();
        }

        private void CarComboHandler_OnComboSuccess(ComboSuccessEventArgs args)
        {
            if (!HasJob()) return;

            CurrentJob.AddJobBonus(args.Reward);
            OnFareBonusEarned?.Invoke(CurrentJob, args.Reward);

            LogX.LogDebug($"On fare bonus earned, {args.Reward}");
        }

        private void NotifyNearbyPassengers()
        {
            // The count never exceeds the buffer length, so no extra Min is needed.
            int detectedPassengerCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                _passengerNotificationRadius,
                _notifyBuffer,
                _passengerLayerMask);

            //? might be good select the closest ones
            //? since in my game there is low number of passengers and _passengerNotificationRadius is small,
            //? notifying closest passengers is overkill
            for (int i = 0; i < detectedPassengerCount; i++)
            {
                if (_notifyBuffer[i].TryGetComponent(out Passenger passenger))
                {
                    passenger.NotifyCarNearby(this);
                    LogX.LogDebug($"Notified passenger {passenger.name} of horn.");
                }
            }
        }

        private void DropOffPassenger()
        {
            if (_currentPassenger == null) return;

            LogX.LogDebug($"Passenger {_currentPassenger.name} dropped off.");

            PassengerJob passengerJob = _currentPassenger.Job;
            Passenger passenger = _currentPassenger;

            _currentPassenger.DropOff();
            _currentPassenger = null;

            // The passenger is managed by the passenger simulation again, so it is
            // cleaned up by distance once the player has moved away.
            ServiceLocator
                .Scene
                .GetService<PassengerManager>()
                .NotifyPassengerDroppedOff(passenger);

            ServiceLocator.Global.GetService<SoundManager>()
                .PlaySound(_passengerPickUpAndDropOffSoundData);

            DestroyPassengerDestinationVfx();

            OnPassengerDroppedOff?.Invoke(passengerJob);
        }

        private void SpawnPassengerDestinationVfx()
        {
            //Add slight offset to avoid z-fighting with the ground
            Vector3 spawnPosition = CurrentJob.DestinationPoint.Position
                + CurrentJob.DestinationPoint.transform.up * 0.15f;

            _passengerDestinationAreaVfx = Instantiate(
                _prefabPassengerDestinationAreaVfx,
                spawnPosition,
                Quaternion.identity);
        }

        private void DestroyPassengerDestinationVfx()
        {
            Destroy(_passengerDestinationAreaVfx);
        }
    }
}