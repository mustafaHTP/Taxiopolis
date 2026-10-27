using UnityEngine;

namespace Game.Utils
{
    public static class CarUtils
    {
        public static float GetSlipDirection(Rigidbody rigidbody)
        {
            return Vector3.Dot(rigidbody.transform.right, rigidbody.linearVelocity.normalized);
        }

        public static float GetRbSpeed(Rigidbody rigidbody)
        {
            return rigidbody.linearVelocity.magnitude;
        }

        public static float GetSpeed(Rigidbody rigidbody)
        {
            return Vector3.Dot(rigidbody.transform.forward, rigidbody.linearVelocity);
        }

        public static float GetSpeedAbs(Rigidbody rigidbody)
        {
            return Mathf.Abs(GetSpeed(rigidbody));
        }

        public static float GetSpeedNormalized(Rigidbody rigidbody, float topSpeed)
        {
            return Mathf.Clamp01(GetSpeed(rigidbody) / topSpeed);
        }

        public static float GetAcceleration(Rigidbody rigidbody, float previousSpeed)
        {
            float currentCarSpeed = GetSpeed(rigidbody);
            float deltaVelocity = currentCarSpeed - previousSpeed;
            float currentAcceleration = deltaVelocity / Time.fixedDeltaTime;

            return currentAcceleration;
        }

        public static float GetAvailableTorque(Rigidbody rigidbody, float motorTorque, float topSpeed, AnimationCurve torqueCurve)
        {
            float carSpeed = Vector3.Dot(rigidbody.transform.forward, rigidbody.linearVelocity);
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed / topSpeed));

            return torqueCurve.Evaluate(normalizedSpeed) * motorTorque;
        }

        /// <summary>
        /// Get the slip angle of the car
        /// </summary>
        /// <returns>Angle between forward and ridigbody velocity</returns>
        public static float GetSlipAngle(Transform transform, Rigidbody rigidbody)
        {
            Vector3 forward = new(transform.forward.x, 0, transform.forward.z); // Ignore Y axis
            Vector3 velocity = new(rigidbody.linearVelocity.x, 0, rigidbody.linearVelocity.z); // Ignore Y axis

            float driftAngle = Vector3.SignedAngle(forward, velocity, transform.up);
            return driftAngle;
        }

        public static Vector3 GetAngularVelocity(Rigidbody rigidbody) => rigidbody.angularVelocity;
        public static float GetAngularVelocityMagnitude(Rigidbody rigidbody) => rigidbody.angularVelocity.magnitude;
        public static void SetAngularVelocity(Rigidbody rigidbody, Vector3 velocity) => rigidbody.angularVelocity = velocity;
    }
}
