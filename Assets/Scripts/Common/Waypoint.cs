using UnityEngine;

namespace Game.Common
{
    public class Waypoint : MonoBehaviour
    {
        [SerializeField] private Color _color = Color.green;
        [SerializeField] private float _radius = 1f;

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        private void OnDrawGizmos()
        {
            Gizmos.color = _color;
            Gizmos.DrawSphere(transform.position, _radius);
        }
    }
}
