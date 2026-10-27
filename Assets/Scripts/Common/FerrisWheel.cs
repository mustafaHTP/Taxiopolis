using Game.Utils;
using UnityEngine;

namespace Game.Common
{
    public class FerrisWheel : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _debugEnableLogging = false;
        [SerializeField] private bool _debugDrawPath = true;
        [SerializeField]
        [Tooltip("The more increases the more circle is smoother")]
        private int _debugPathSegments = 36;
        [SerializeField] private Color32 _debugPathColor;

        [Header("Ferris Wheel Settings")]
        [SerializeField]
        [Tooltip("The axis around which the ferris wheel rotates." +
            "It is determined by by the selected rotation axis of rotation wheel.")]
        private RotationAxis _rotationAxis;
        [SerializeField] private Transform[] _cabinTransforms;
        [SerializeField] private Transform _rotationWheel;
        [SerializeField] private float _rotationSpeed = 10f;

        private Vector3 _cosPlane = Vector3.one;
        private Vector3 _sinPlane = Vector3.one;
        /// <summary>
        /// It is determined by the selected rotation axis of rotation wheel.
        /// It must be global axis because it is used in rotation wheel Rotate method.
        /// If you use another rotate method, you may need to change it rotation wheel local axis.
        /// </summary>
        private Vector3 _rotationAxisVector = Vector3.one;

        private enum RotationAxis
        {
            X_Axis,
            Y_Axis,
            Z_Axis
        }

        private float _radius;
        private FerrisWheelCabin[] _ferrisWheelCabins;

        private class FerrisWheelCabin
        {
            public Transform CabinTransform;
            public float CurrentTheta;
        }

        private void OnDrawGizmos()
        {
            if (!_debugDrawPath) return;
            if (_cabinTransforms == null || _cabinTransforms.Length == 0 || _rotationWheel == null) return;

            Gizmos.color = _debugPathColor;

            /* cos and sin plane vectors' orders matters for correct drawing
             * to find correct planes for rotating axis
             * use left hand rule and look at the direction of the rotating axis vector
             */
            Vector3 cosPlane = Vector3.one;
            Vector3 sinPlane = Vector3.one;
            switch (_rotationAxis)
            {
                case RotationAxis.X_Axis:
                    cosPlane = _rotationWheel.forward;
                    sinPlane = _rotationWheel.up;
                    break;
                case RotationAxis.Y_Axis:
                    cosPlane = _rotationWheel.right;
                    sinPlane = _rotationWheel.forward;
                    break;
                case RotationAxis.Z_Axis:
                    cosPlane = _rotationWheel.right;
                    sinPlane = _rotationWheel.up;
                    break;
            }

            float radius = GetRadius();
            float angleStep = 360f / _debugPathSegments;
            float currentTheta = 0f;
            Vector3 centerPosition = _rotationWheel.position;

            float xPoint = Mathf.Cos(Mathf.Deg2Rad * currentTheta) * radius;
            float yPoint = Mathf.Sin(Mathf.Deg2Rad * currentTheta) * radius;
            Vector3 previousPoint = (cosPlane * xPoint) + (sinPlane * yPoint) + centerPosition;
            currentTheta += angleStep;

            for (int i = 0; i < _debugPathSegments; ++i)
            {
                xPoint = Mathf.Cos(Mathf.Deg2Rad * currentTheta) * radius;
                yPoint = Mathf.Sin(Mathf.Deg2Rad * currentTheta) * radius;
                Vector3 currentPoint = (cosPlane * xPoint) + (sinPlane * yPoint) + centerPosition;

                Gizmos.DrawLine(previousPoint, currentPoint);
                Gizmos.DrawLine(_rotationWheel.position, currentPoint);
                previousPoint = currentPoint;
                currentTheta += angleStep;
            }

        }

        private void Start()
        {
            if (_cabinTransforms == null
                || _cabinTransforms.Length == 0
                || _cabinTransforms[0] == null
                || _rotationWheel == null)
            {
                LogX.LogError("FerrisWheel: Cabin transforms or rotation wheel is not assigned.");
                return;
            }

            _ferrisWheelCabins = new FerrisWheelCabin[_cabinTransforms.Length];
            _radius = GetRadius();

            switch (_rotationAxis)
            {
                case RotationAxis.X_Axis:
                    _cosPlane = _rotationWheel.forward;
                    _sinPlane = _rotationWheel.up;
                    _rotationAxisVector = Vector3.right;
                    break;
                case RotationAxis.Y_Axis:
                    _cosPlane = _rotationWheel.right;
                    _sinPlane = _rotationWheel.forward;
                    _rotationAxisVector = Vector3.up;
                    break;
                case RotationAxis.Z_Axis:
                    _cosPlane = _rotationWheel.right;
                    _sinPlane = _rotationWheel.up;
                    _rotationAxisVector = Vector3.forward;
                    break;
            }

            // Initialize all cabins
            for (int i = 0; i < _cabinTransforms.Length; i++)
            {
                // Calculate initial angle for the cabin
                // It means it helps to find where the cabin is located on the circle
                float cabinCurrentTheta = Vector3.SignedAngle(
                    _cosPlane,
                    _cabinTransforms[i].position - _rotationWheel.position,
                    _rotationAxisVector);

                _ferrisWheelCabins[i] = new FerrisWheelCabin
                {
                    CabinTransform = _cabinTransforms[i],
                    CurrentTheta = cabinCurrentTheta
                };

                if (_debugEnableLogging) LogX.LogInfo($"Cabin initial angle: {cabinCurrentTheta} degrees");
            }
        }

        private void Update()
        {
            // Calculate new position for all cabins
            for (int i = 0; i < _ferrisWheelCabins.Length; i++)
            {
                FerrisWheelCabin currentFerrisWheelCabin = _ferrisWheelCabins[i];
                currentFerrisWheelCabin.CurrentTheta += _rotationSpeed * Time.deltaTime;
                currentFerrisWheelCabin.CurrentTheta = Mathf.Repeat(currentFerrisWheelCabin.CurrentTheta, 360f); // Keep the angle within 0-360 degrees
                float xPos = Mathf.Cos(Mathf.Deg2Rad * currentFerrisWheelCabin.CurrentTheta) * _radius;
                float yPos = Mathf.Sin(Mathf.Deg2Rad * currentFerrisWheelCabin.CurrentTheta) * _radius;
                Vector3 newCabinPosition = (_cosPlane * xPos) + (_sinPlane * yPos) + _rotationWheel.position;

                currentFerrisWheelCabin.CabinTransform.position = newCabinPosition;
            }

            // Also rotate the wheel visually
            _rotationWheel.Rotate(_rotationAxisVector, _rotationSpeed * Time.deltaTime);
        }

        private float GetRadius()
        {
            if (_cabinTransforms == null || _cabinTransforms.Length == 0) return 0f;

            return Vector3.Distance(_rotationWheel.position, _cabinTransforms[0].position);
        }
    }
}
