using UnityEngine;

namespace Game.Systems.PassengerSystem
{
    public class PassengerWaitingPoint : MonoBehaviour
    {
        private PassengerZone _zone;

        public PassengerZone Zone => _zone;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        public bool HasZone
        {
            get
            {
                if (_zone == null)
                {
                    _zone = GetComponentInParent<PassengerZone>();
                }

                return _zone != null;
            }
        }
    }
}
