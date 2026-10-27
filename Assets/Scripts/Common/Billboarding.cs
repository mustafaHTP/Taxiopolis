using UnityEngine;

namespace Game.Common
{
    public class Billboarding : MonoBehaviour
    {
        private void Update()
        {
            Quaternion targetRotation = Camera.main.transform.rotation;
            transform.LookAt(
                transform.position + targetRotation * Vector3.forward,
                targetRotation * Vector3.up);
        }
    }
}
