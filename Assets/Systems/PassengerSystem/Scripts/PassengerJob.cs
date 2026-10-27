using Game.Systems.PassengerSystem.Passengers;

namespace Game.Systems.PassengerSystem
{
    public enum JobState { Waiting, InProgress, Completed, Failed }

    public class PassengerJob
    {
        public PassengerJob(float tripTime, float tripDistance, float pickupBonusTime, int fare, Passenger passenger, PassengerWaitingPoint waitingPoint, PassengerDestinationPoint destinationPoint)
        {
            TripTime = tripTime;
            TripDistance = tripDistance;
            PickupBonusTime = pickupBonusTime;
            Fare = fare;
            Passenger = passenger;
            WaitingPoint = waitingPoint;
            DestinationPoint = destinationPoint;

            TimeLeft = tripTime;
            State = JobState.Waiting;
        }

        public void AddJobBonus(int reward)
        {
            _reward += reward;
        }

        public float TripTime { get; }
        public float TripDistance { get; }
        public float PickupBonusTime { get; }
        public int Fare { get; }
        public int TotalFare => Fare + _reward;
        public Passenger Passenger { get; }
        public PassengerWaitingPoint WaitingPoint { get; }
        public PassengerDestinationPoint DestinationPoint { get; }
        public CarPassengerHandler Car { get; private set; }
        public JobState State { get; private set; }
        public float TimeLeft { get; private set; }
        public PassengerJobResultTier PassengerJobResultTier { get; private set; }

        private int _reward;

        public void Start(CarPassengerHandler car)
        {
            Car = car;
            State = JobState.InProgress;
        }

        /// <summary>
        /// Finishes the job and stores the result tier.
        /// If the time already ran out, the state stays Failed (it is NOT overwritten with Completed),
        /// so the passenger can still react to the failure (AngryState).
        /// </summary>
        public void Complete(PassengerJobResultTier passengerJobResultTier)
        {
            if (State != JobState.Failed)
            {
                State = JobState.Completed;
            }

            PassengerJobResultTier = passengerJobResultTier;
        }

        public void Tick(float dt)
        {
            if (State != JobState.InProgress) return;

            TimeLeft -= dt;
            if (TimeLeft <= 0)
            {
                State = JobState.Failed;
            }
        }
    }
}