using System.Linq;
using UnityEngine;

namespace RoadSystem.Utils
{
    public static class RoadSystemUtils
    {
        /// <summary>
        /// It checks if there is any connection to the given road point in the scene.
        /// </summary>
        public static bool IsConnectedFromAny(RoadPoint targetRoadPoint)
        {
            // 1. Get all road points in the scene
            var roadPoints = GameObject.FindObjectsByType<RoadPoint>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            if (roadPoints == null || roadPoints.Length == 0)
            {
                return false;
            }

            // 2. Check if the any road point is connected to the given road point
            foreach (var point in roadPoints)
            {
                if (point != targetRoadPoint && point.Connections.Any(c => c == targetRoadPoint))
                {
                    return true;
                }
            }

            // 3. If no connections found, return false
            return false;
        }

        /// <summary>
        /// It removes the given road point from all connections of other road points in the scene.
        /// </summary>
        public static void RemoveConnection(RoadPoint targetRoadPoint)
        {
            // 1. Get all road points in the scene
            var roadPoints = GameObject.FindObjectsByType<RoadPoint>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            if (roadPoints == null || roadPoints.Length == 0)
            {
                return;
            }

            // 2. Remove the target road point from all connections of other road points
            foreach (var point in roadPoints)
            {
                if (point != targetRoadPoint && point.Connections.Any(c => c == targetRoadPoint))
                {
                    point.Connections.Remove(targetRoadPoint);
                }
            }
        }

        /// <summary>
        /// It projects the given position onto the line segment defined by two road points.
        /// </summary>
        public static Vector3 ProjectionOn(RoadPoint a, RoadPoint b, Vector3 position)
        {
            Vector3 ab = b.Position - a.Position;
            Vector3 ap = position - a.Position;

            float t = Vector3.Dot(ab, ap) / ab.sqrMagnitude;

            t = Mathf.Clamp01(t);

            return a.Position + t * ab;
        }

        public static bool IsSpawnPoint(RoadPoint roadPoint)
        {
            if (roadPoint == null)
            {
                return false;
            }

            if (!roadPoint.TryGetComponent<RoadSpawnPoint>(out var _))
            {
                return false;
            }

            return true;
        }

        public static void RemoveNonExistingRoadPoints(Road road)
        {
            if (road == null || road.RoadPoints.Count == 0) return;

            road.RoadPoints.RemoveAll(roadPoint => roadPoint == null);
        }

        public static void RemoveNonExistingConnections(Road road)
        {
            if (road == null || road.RoadPoints.Count == 0)
            {
                return;
            }

            for (int i = 0; i < road.RoadPoints.Count; i++)
            {
                var roadPoint = road.RoadPoints[i];
                if (roadPoint == null)
                {
                    continue;
                }

                RemoveNonExistingConnections(roadPoint);
            }
        }

        public static void RemoveNonExistingConnections(RoadPoint roadPoint)
        {
            if (roadPoint == null || roadPoint.Connections.Count == 0)
            {
                return;
            }

            roadPoint.Connections.RemoveAll(connectedRoadPoint => connectedRoadPoint == null);
        }

        public static int? GetClosestIndexOn(Road road, Vector3 position)
        {
            int? closestIndex = null;
            float closestDistance = float.MaxValue;
            if (road == null || road.RoadPoints.Count == 0)
            {
                return closestIndex;
            }

            for (int i = 0; i < road.RoadPoints.Count - 1; i++)
            {
                var aPoint = road.RoadPoints[i];
                var bPoint = road.RoadPoints[i + 1];
                var projection = ProjectionOn(aPoint, bPoint, position);
                var distance = Vector3.Distance(position, projection);
                if (distance < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance;
                }
            }

            return closestIndex;
        }

        public static Vector3? GetClosestPositionOn(Road road, Vector3 position)
        {
            Vector3? closestPosition = null;
            float closestDistance = float.MaxValue;

            if (road == null || road.RoadPoints.Count == 0)
            {
                return closestPosition;
            }

            for (int i = 0; i < road.RoadPoints.Count - 1; i++)
            {
                var aPoint = road.RoadPoints[i];
                var bPoint = road.RoadPoints[i + 1];

                var projection = ProjectionOn(aPoint, bPoint, position);

                var distance = Vector3.Distance(position, projection);
                if (distance < closestDistance)
                {
                    closestPosition = projection;
                    closestDistance = distance;
                }
            }

            return closestPosition;
        }

        public static void UseEventIfNeeded(Event currentEvent)
        {
            if (currentEvent != null
                && currentEvent.type != EventType.Repaint
                && currentEvent.type != EventType.Layout)
            {
                currentEvent.Use();
            }
        }

        public static void EvaluateRotations(Road road)
        {
            if (road == null || road.RoadPoints.Count == 0 || road.RoadPoints.Count == 1)
            {
                return;
            }

            // Iterate through the road points and set their rotations
            // except the last one, which faces the same of the last segment
            for (int i = 0; i < road.RoadPoints.Count - 1; i++)
            {
                var aPoint = road.RoadPoints[i];
                var bPoint = road.RoadPoints[i + 1];
                if (aPoint == null || bPoint == null)
                {
                    continue;
                }
                // Calculate the direction vector from point A to point B
                Vector3 direction = (bPoint.Position - aPoint.Position).normalized;
                // Set the rotation of point A to look towards point B
                aPoint.transform.rotation = Quaternion.LookRotation(direction);
            }

            // Set the rotation of the last point to face the same direction of the last segment
            var penultimateLastPoint = road.RoadPoints[^2];
            var forwardPenultimateLastPoint = penultimateLastPoint.transform.forward;
            var newRotationOfLastPoint = Quaternion.LookRotation(forwardPenultimateLastPoint, Vector3.up);
            var lastPoint = road.RoadPoints[^1];
            if (lastPoint != null)
            {
                lastPoint.transform.rotation = newRotationOfLastPoint;
            }
        }
    }
}
