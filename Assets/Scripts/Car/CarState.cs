using UnityEngine;

namespace Game.Car
{
    public readonly struct CarState
    {
        public CarState(float speed,
                        float speedNormalized,
                        float rbSpeed,
                        Vector3 angularVelocity,
                        float slipDirection,
                        float slipAngle,
                        float availableTorque,
                        float acceleration,
                        bool isDrifting,
                        CarEnvCheckResultContainer carEnvCheckResultContainer)
        {
            Speed = speed;
            SpeedNormalized = speedNormalized;
            RbSpeed = rbSpeed;
            AngularVelocity = angularVelocity;
            SlipDirection = slipDirection;
            SlipAngle = slipAngle;
            AvailableTorque = availableTorque;
            Acceleration = acceleration;
            IsDrifting = isDrifting;
            CarEnvCheckResultContainer = carEnvCheckResultContainer;
        }

        public float Speed { get; }
        public float SpeedNormalized { get; }
        public float RbSpeed { get; }
        public Vector3 AngularVelocity { get; }
        public float SlipDirection { get; }
        public float SlipAngle { get; }
        public float AvailableTorque { get; }
        public float Acceleration { get; }
        public bool IsDrifting { get; }
        public CarEnvCheckResultContainer CarEnvCheckResultContainer { get; }
    }
}
