using UnityEngine;

namespace RoadSystem.Utils
{
    public static class RoadDraw
    {
        public static void DrawGizmosArrow(
            Vector3 start,
            Vector3 directionNormalized,
            float baseLength,
            float headAngle,
            float headLength,
            bool isClosedArrow = true)
        {
            // Draw the main line of the arrow
            Gizmos.DrawRay(start, directionNormalized * baseLength);

            Vector3 end = start + (directionNormalized * baseLength);

            // Build a rotation that points in the direction of the arrow
            Quaternion rot = Quaternion.LookRotation(directionNormalized);

            // Arrowhead directions in local space
            Vector3 leftHead = rot * Quaternion.Euler(0, headAngle, 0) * -Vector3.forward;
            Vector3 rightHead = rot * Quaternion.Euler(0, -headAngle, 0) * -Vector3.forward;

            Gizmos.DrawRay(end, leftHead * headLength);
            Gizmos.DrawRay(end, rightHead * headLength);

            if (!isClosedArrow) return;

            Vector3 leftTip = end + leftHead * headLength;
            Vector3 rightTip = end + rightHead * headLength;

            // Draw the closing line between the two arrowhead tips
            Gizmos.DrawLine(leftTip, rightTip);
        }

        public static void DebugRotatedBox(Vector3 center, Vector3 size, Quaternion rotation, Color color, float duration = 0, bool depthTest = true)
        {
            Vector3 halfSize = size * 0.5f;

            // Calculate the 8 corners in local space
            Vector3[] corners = new Vector3[8]
            {
                new(-halfSize.x, -halfSize.y, -halfSize.z),
                new(halfSize.x, -halfSize.y, -halfSize.z),
                new(halfSize.x, -halfSize.y, halfSize.z),
                new(-halfSize.x, -halfSize.y, halfSize.z),
                new(-halfSize.x, halfSize.y, -halfSize.z),
                new(halfSize.x, halfSize.y, -halfSize.z),
                new(halfSize.x, halfSize.y, halfSize.z),
                new(-halfSize.x, halfSize.y, halfSize.z)
            };

            // Transform corners to world space
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = rotation * corners[i] + center;
            }

            // Draw bottom square
            Debug.DrawLine(corners[0], corners[1], color, duration, depthTest);
            Debug.DrawLine(corners[1], corners[2], color, duration, depthTest);
            Debug.DrawLine(corners[2], corners[3], color, duration, depthTest);
            Debug.DrawLine(corners[3], corners[0], color, duration, depthTest);

            // Draw top square
            Debug.DrawLine(corners[4], corners[5], color, duration, depthTest);
            Debug.DrawLine(corners[5], corners[6], color, duration, depthTest);
            Debug.DrawLine(corners[6], corners[7], color, duration, depthTest);
            Debug.DrawLine(corners[7], corners[4], color, duration, depthTest);

            // Draw vertical lines
            Debug.DrawLine(corners[0], corners[4], color, duration, depthTest);
            Debug.DrawLine(corners[1], corners[5], color, duration, depthTest);
            Debug.DrawLine(corners[2], corners[6], color, duration, depthTest);
            Debug.DrawLine(corners[3], corners[7], color, duration, depthTest);
        }
    }
}
