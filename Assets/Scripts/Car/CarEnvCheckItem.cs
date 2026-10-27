using UnityEngine;

namespace Game.Car
{
    public readonly struct CarEnvCheckResultContainer
    {
        public CarEnvCheckResultContainer(CarEnvCheckResult left, CarEnvCheckResult right, CarEnvCheckResult front, CarEnvCheckResult back, CarEnvCheckResult up, CarEnvCheckResult down)
        {
            Left = left;
            Right = right;
            Front = front;
            Back = back;
            Up = up;
            Down = down;
        }

        public CarEnvCheckResult Left { get; }
        public CarEnvCheckResult Right { get; }
        public CarEnvCheckResult Front { get; }
        public CarEnvCheckResult Back { get; }
        public CarEnvCheckResult Up { get; }
        public CarEnvCheckResult Down { get; }
    }

    /// <summary>
    /// Stores the result of an environment check for the car.
    /// </summary>
    public readonly struct CarEnvCheckResult
    {
        public CarEnvCheckResult(RaycastHit hit, bool isHit)
        {
            Hit = hit;
            IsHit = isHit;
        }

        //? need to store hit data?
        public RaycastHit Hit { get; }
        public bool IsHit { get; }
    }
}
