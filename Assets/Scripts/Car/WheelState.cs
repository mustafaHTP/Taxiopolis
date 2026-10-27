using UnityEngine;

namespace Game.Car
{
    public class WheelState
    {
        private RaycastHit _hit;
        private bool _isGrounded;
        private float _rollingAngle;

        public RaycastHit Hit { get => _hit; set => _hit = value; }
        public bool IsGrounded { get => _isGrounded; set => _isGrounded = value; }
        public float RollingAngle { get => _rollingAngle; set => _rollingAngle = value; }
    }
}
