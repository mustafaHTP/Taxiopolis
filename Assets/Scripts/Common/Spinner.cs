using Game.Utils;
using UnityEngine;

namespace Game.Common
{
    public class Spinner : MonoBehaviour
    {
        [SerializeField] private bool _canRotate = true;
        [SerializeField] private Vector3 _rotationAmountVector;

        private void Update()
        {
            if (!_canRotate) return;

            float xRotationDelta = _rotationAmountVector.x * Time.deltaTime;
            float yRotationDelta = _rotationAmountVector.y * Time.deltaTime;
            float zRotationDelta = _rotationAmountVector.z * Time.deltaTime;

            transform.Rotate(
                xRotationDelta,
                yRotationDelta,
                zRotationDelta);
        }

        [ContextMenu("Reset Rotation")]
        private void ResetRotation()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}
