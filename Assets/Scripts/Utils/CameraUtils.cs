using UnityEngine;

namespace Game.Utils
{
    public static class CameraUtils
    {
        // Convenience version, fine for one-off checks.
        public static bool IsPointInCameraFrustum(Camera cam, Vector3 worldPos)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);

            return IsPointInFrustum(planes, worldPos);
        }

        // Use this in loops: calculate the planes ONCE, test many points.
        public static bool IsPointInFrustum(Plane[] planes, Vector3 worldPos)
        {
            for (int i = 0; i < planes.Length; i++)
                if (planes[i].GetDistanceToPoint(worldPos) < 0f)
                    return false;
            return true;
        }
    }
}