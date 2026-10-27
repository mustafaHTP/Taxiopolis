using UnityEngine;

namespace Game.Utils
{
    /// <summary>
    /// Utility drawing helpers. Contains two nested static helpers to separate runtime debug drawing
    /// (uses UnityEngine.Debug) and editor/runtime gizmo drawing (uses UnityEngine.Gizmos).
    /// </summary>
    public static class DrawX
    {
        public static class Debug
        {
            /// <summary>
            /// Draws a circle using UnityEngine.Debug.DrawLine. Useful for runtime visual debugging.
            /// </summary>
            public static void DrawCircle(Vector3 center, Vector3 normal, float radius, UnityEngine.Color color, int segments = 24, float duration = 0f, bool depthTest = true)
            {
                if (segments < 3) segments = 3;

                // Build orthonormal basis
                Vector3 n = normal.normalized;
                Vector3 tangent = Vector3.Cross(n, Vector3.up);
                if (tangent.sqrMagnitude < 1e-6f)
                {
                    tangent = Vector3.Cross(n, Vector3.right);
                }
                tangent.Normalize();
                Vector3 bitangent = Vector3.Cross(n, tangent);

                Vector3 prevPoint = center + (tangent * radius);
                float delta = 2f * Mathf.PI / segments;
                for (int i = 1; i <= segments; i++)
                {
                    float angle = i * delta;
                    Vector3 nextPoint = center + (tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle)) * radius;
                    UnityEngine.Debug.DrawLine(prevPoint, nextPoint, color, duration, depthTest);
                    prevPoint = nextPoint;
                }
            }

            public static void DrawBox(Vector3 center, Vector3 halfExtents, Quaternion rotation, Color color, float duration = 0f, bool depthTest = true)
            {
                Vector3[] corners = new Vector3[8];
                Vector3 extX = rotation * new Vector3(halfExtents.x, 0, 0);
                Vector3 extY = rotation * new Vector3(0, halfExtents.y, 0);
                Vector3 extZ = rotation * new Vector3(0, 0, halfExtents.z);
                corners[0] = center + extX + extY + extZ;
                corners[1] = center + extX + extY - extZ;
                corners[2] = center + extX - extY + extZ;
                corners[3] = center + extX - extY - extZ;
                corners[4] = center - extX + extY + extZ;
                corners[5] = center - extX + extY - extZ;
                corners[6] = center - extX - extY + extZ;
                corners[7] = center - extX - extY - extZ;
                // Draw edges
                for (int i = 0; i < 4; i++)
                {
                    UnityEngine.Debug.DrawLine(corners[i], corners[i + 4], color, duration, depthTest);
                    UnityEngine.Debug.DrawLine(corners[i], corners[(i + 1) % 4], color, duration, depthTest);
                    UnityEngine.Debug.DrawLine(corners[i + 4], corners[((i + 1) % 4) + 4], color, duration, depthTest);
                }
            }
        }

        public static class Gizmos
        {
            public static void DrawCircle(Vector3 center, Vector3 normal, Vector3 right, float radius, int segments = 24)
            {
                if (segments < 4) segments = 3;

                float angleStep = 360f / segments;
                Vector3 crossVector = Vector3.Cross(normal.normalized, right.normalized).normalized;

                Vector3 forward = crossVector;

                float currentAngle = 0f;

                Vector3 x = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * radius * right;
                Vector3 y = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * radius * forward;

                Vector3 prevPoint = x + y + center;
                for (int i = 0; i < segments; i++)
                {
                    currentAngle += angleStep;
                    x = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * radius * right;
                    y = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * radius * forward;

                    Vector3 nextPoint = x + y + center;

                    UnityEngine.Gizmos.DrawLine(prevPoint, nextPoint);

                    prevPoint = nextPoint;
                }

            }
        }
    }
}
