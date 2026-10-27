using UnityEngine;

namespace Game.Car
{
    public class CenterOfMassController : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private Transform _COMTransform;
        [SerializeField] private bool _drawCOM = true;
        [SerializeField] private Color32 _drawCOMColor = new(0, 0, 255, 255);
        [SerializeField] private float _drawCOMRadius = 0.3f;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            _rigidbody.centerOfMass = _COMTransform.localPosition;
        }

        private void OnDrawGizmos()
        {
            if (_rigidbody == null) return;
            if (!_drawCOM) return;

            Gizmos.color = _drawCOMColor;
            Gizmos.DrawSphere(_rigidbody.worldCenterOfMass, _drawCOMRadius);
        }
    }
}
