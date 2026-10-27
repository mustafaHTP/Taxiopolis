using Framework.Grid;
using Game.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RoadSystem
{
    /// <summary>
    /// Represents a road network component within a Unity scene, providing functionality for managing and interacting
    /// with roads and their connections. Move all roads in a scene under a GameObject with this component to
    /// </summary>
    /// <remarks>Attach this component to a GameObject to enable road network features such as path finding.
    /// The specific capabilities depend on additional methods and properties
    /// defined in this class or its derived types.</remarks>
    public class RoadNetwork : MonoBehaviour
    {
        [Header("Road Grid Settings")]
        [SerializeField] private float _roadPointGridCellSize = 10f;
        [SerializeField] private int _pathFindingOpsLimitOnFrame = 5;

        [Header("Road Spawn Point Grid")]
        [Tooltip("Affects speed only, not correctness. Half to full of the traffic max spawn distance is a good start.")]
        [SerializeField] private float _roadSpawnPointGridCellSize = 30f;

        public enum PathfindingAlgorithm
        {
            Custom,
            Dijkstra,
            AStar
        }

        private struct DebugCell
        {
            public int Count;
            public float SumY;
        }

        private readonly Dictionary<Vector2Int, DebugCell> _debugSpawnPointCells = new();

        private RoadPoint[] _roadPoints;
        private RoadSpawnPoint[] _roadSpawnPoints;
        private SpatialHashGrid<RoadPoint> _roadPointGrid;
        private SpatialHashGrid<RoadSpawnPoint> _roadSpawnPointGrid;

        public RoadPoint[] RoadPoints => _roadPoints;
        public RoadSpawnPoint[] RoadSpawnPoints => _roadSpawnPoints;

        private class PathNode
        {
            public RoadPoint RoadPoint { get; set; }
            public RoadPoint CameFrom { get; set; }

            public float GCost { get; set; }
            public float HCost { get; set; }
            public float FCost { get; private set; }

            public void CalculateFCost()
            {
                FCost = GCost + HCost;
            }
        }

        private class PathNodeComparer : IComparer<PathNode>
        {
            public int Compare(PathNode x, PathNode y)
            {
                int fCostComparison = x.FCost.CompareTo(y.FCost);
                if (fCostComparison != 0) return fCostComparison;

                int hCostComparison = x.HCost.CompareTo(y.HCost);
                if (hCostComparison != 0) return hCostComparison;

                return x.RoadPoint.GetInstanceID().CompareTo(y.RoadPoint.GetInstanceID());
            }
        }

        public void ConstructRoadGraph()
        {
            _roadPoints = FindObjectsByType<RoadPoint>(FindObjectsSortMode.None);
            _roadSpawnPoints = FindObjectsByType<RoadSpawnPoint>(FindObjectsSortMode.None);

            ConstructRoadGrid();
        }

        /// <summary>
        /// Fills <paramref name="results"/> (cleared first) with the road spawn points in the grid cells
        /// around <paramref name="center"/>. This is a coarse result: do the exact distance
        /// check yourself. Pass a reused list so the query does not allocate.
        /// Returns nothing until ConstructRoadGraph has run.
        /// </summary>
        public void QueryNearbySpawnPoints(Vector3 center, float radius, List<RoadSpawnPoint> results)
        {
            results.Clear();

            if (_roadSpawnPointGrid == null) return;

            _roadSpawnPointGrid.QueryRadius(center, radius, results);
        }

        private float HeuristicCostEstimate(RoadPoint a, RoadPoint b)
        {
            return Vector3.Distance(a.Position, b.Position);
        }

        public IEnumerator FindPathAStarCoroutine(Vector3 startPosition, Vector3 targetPosition, Action<PathData> onComplete)
        {
            List<RoadPoint> startRoadPointCandidates = GetNeighbourRoadPoints(startPosition);
            List<RoadPoint> targetRoadPointCandidates = GetNeighbourRoadPoints(targetPosition);

            if (startRoadPointCandidates == null
                || targetRoadPointCandidates == null
                || startRoadPointCandidates.Count == 0
                || targetRoadPointCandidates.Count == 0)
            {
                LogX.LogWarning("Could not find valid start or target road points for the given positions.");

                onComplete?.Invoke(null);

                yield break;
            }

            List<PathData> candidatePaths = new();
            int opsInThisFrame = 0;
            for (int i = 0; i < startRoadPointCandidates.Count; i++)
            {
                for (int j = 0; j < targetRoadPointCandidates.Count; j++)
                {
                    var path = FindPathAStar(
                        startRoadPointCandidates[i],
                        targetRoadPointCandidates[j]);

                    if (path != null)
                    {
                        candidatePaths.Add(path);
                    }

                    ++opsInThisFrame;

                    if (opsInThisFrame >= _pathFindingOpsLimitOnFrame)
                    {
                        opsInThisFrame = 0;
                        yield return null;
                    }
                }
            }

            // Find the best path based on distance
            PathData shortestPath;
            if (candidatePaths.Count > 0)
            {
                shortestPath = candidatePaths
                    .OrderBy(p => p.Length)
                    .First();

                onComplete?.Invoke(shortestPath);
            }
            else
            {
                LogX.LogWarning("No valid paths found between the given positions.");

                onComplete?.Invoke(null);
            }
        }

        private PathData FindPathAStar(RoadPoint startRoadPoint, RoadPoint targetRoadPoint)
        {
            if (startRoadPoint == null || targetRoadPoint == null)
            {
                LogX.LogError("Start node or target node is null. Cannot find path.");
                return null;
            }

            if (startRoadPoint == targetRoadPoint)
            {
                LogX.LogWarning("Start and target road points are the same. No path needed.");
                return new PathData(new Vector3[] { startRoadPoint.Position }, 0f);
            }

            Dictionary<RoadPoint, PathNode> pathNodes = new();
            SortedSet<PathNode> openSet = new(new PathNodeComparer());
            HashSet<RoadPoint> closedSet = new();

            PathNode startNode = new PathNode
            {
                RoadPoint = startRoadPoint,
                GCost = 0f,
                HCost = HeuristicCostEstimate(startRoadPoint, targetRoadPoint),
                CameFrom = null
            };
            startNode.CalculateFCost();
            pathNodes[startRoadPoint] = startNode;
            openSet.Add(startNode);

            // keep continue to traverse while there is nodes to visit
            while (openSet.Count > 0)
            {
                PathNode currentNode = openSet.Min;
                RoadPoint currentRoadPoint = currentNode.RoadPoint;

                if (currentRoadPoint == targetRoadPoint)
                {
                    return ReconstructPathAStar(targetRoadPoint, pathNodes);
                }

                // mark current node as visited
                openSet.Remove(currentNode);
                closedSet.Add(currentRoadPoint);

                foreach (var neighbour in currentRoadPoint.Neighbours)
                {
                    if (closedSet.Contains(neighbour)) continue;

                    float tentativeGCost = pathNodes[currentRoadPoint].GCost +
                        Vector3.Distance(currentRoadPoint.Position, neighbour.Position);

                    if (!pathNodes.ContainsKey(neighbour))
                    {
                        PathNode neighbourNode = new PathNode
                        {
                            RoadPoint = neighbour,
                            GCost = tentativeGCost,
                            HCost = HeuristicCostEstimate(neighbour, targetRoadPoint),
                            CameFrom = currentRoadPoint
                        };
                        neighbourNode.CalculateFCost();
                        pathNodes[neighbour] = neighbourNode;
                        openSet.Add(neighbourNode);
                    }
                    else if (tentativeGCost < pathNodes[neighbour].GCost)
                    {
                        PathNode neighbourNode = pathNodes[neighbour];
                        openSet.Remove(neighbourNode);

                        neighbourNode.GCost = tentativeGCost;
                        neighbourNode.CameFrom = currentRoadPoint;
                        neighbourNode.CalculateFCost();

                        openSet.Add(neighbourNode);
                    }
                }
            }

            LogX.LogWarning($"No path found from {startRoadPoint.name} to {targetRoadPoint.name}");
            return null;
        }

        private PathData ReconstructPathAStar(RoadPoint targetRoadPoint, Dictionary<RoadPoint, PathNode> pathNodes)
        {
            List<Vector3> path = new();
            RoadPoint currentRoadPoint = targetRoadPoint;
            float pathLength = 0f;

            while (currentRoadPoint != null)
            {
                path.Add(currentRoadPoint.Position);

                PathNode currentNode = pathNodes[currentRoadPoint];
                RoadPoint previousRoadPoint = currentNode.CameFrom;

                if (previousRoadPoint != null)
                {
                    pathLength += Vector3.Distance(currentRoadPoint.Position, previousRoadPoint.Position);
                }

                currentRoadPoint = previousRoadPoint;
            }

            path.Reverse();

            return new PathData(path.ToArray(), pathLength);
        }

        private List<RoadPoint> GetNeighbourRoadPoints(Vector3 targetPosition)
        {
            if (_roadPointGrid == null) return null;

            // A new list per call is intentional. Pathfinding runs on demand, not every tick,
            // and FindPathAStarCoroutine needs the start and target results at the same time,
            // so a shared reused list would be overwritten by the second call.
            List<RoadPoint> results = new();
            _roadPointGrid.QueryRadius(targetPosition, _roadPointGridCellSize, results);

            return results;
        }

        public PathData FindPathCustom(Vector3 startPosition, Vector3 targetPosition)
        {
            RoadPoint startRoadPoint = GetClosestRoadPoint(startPosition);
            RoadPoint targetRoadPoint = GetClosestRoadPoint(targetPosition);
            if (startRoadPoint == null || targetRoadPoint == null)
            {
                LogX.LogError("Could not find valid start or target road points for the given positions.");
                return null;
            }
            return FindPathCustom(startRoadPoint, targetRoadPoint);
        }

        private PathData FindPathCustom(RoadPoint startRoadPoint, RoadPoint targetRoadPoint)
        {
            HashSet<RoadPoint> visitedRoadPoints = new();
            Queue<RoadPoint> roadPointsToVisit = new();
            Dictionary<RoadPoint, RoadPoint> cameFrom = new();

            // Check if start and target road points are the same
            if (startRoadPoint == targetRoadPoint)
            {
                LogX.LogWarning("Start and target road points are the same. No path needed.");

                return new PathData(new Vector3[] { startRoadPoint.Position }, 0f);
            }

            RoadPoint currentRoadPoint = startRoadPoint;

            RoadPoint closestRoadPoint = startRoadPoint;
            float minDistanceSqr = Vector3.SqrMagnitude(targetRoadPoint.Position - currentRoadPoint.Position);

            cameFrom[currentRoadPoint] = null;
            visitedRoadPoints.Add(currentRoadPoint);

            for (int i = 0; i < currentRoadPoint.Neighbours.Count; ++i)
            {
                cameFrom[currentRoadPoint.Neighbours[i]] = currentRoadPoint;
                roadPointsToVisit.Enqueue(currentRoadPoint.Neighbours[i]);
            }

            while (roadPointsToVisit.Count > 0)
            {
                currentRoadPoint = roadPointsToVisit.Dequeue();

                float distanceSqr =
                    Vector3.SqrMagnitude(targetRoadPoint.Position - currentRoadPoint.Position);

                if (distanceSqr < minDistanceSqr)
                {
                    minDistanceSqr = distanceSqr;
                    closestRoadPoint = currentRoadPoint;
                }

                for (int i = 0; i < currentRoadPoint.Neighbours.Count; ++i)
                {
                    if (visitedRoadPoints.Contains(currentRoadPoint.Neighbours[i])) continue;

                    cameFrom[currentRoadPoint.Neighbours[i]] = currentRoadPoint;
                    roadPointsToVisit.Enqueue(currentRoadPoint.Neighbours[i]);
                }

                visitedRoadPoints.Add(currentRoadPoint);
            }

            if (closestRoadPoint == startRoadPoint)
            {
                LogX.LogWarning("No path found from start position to target position.");

                return null;
            }

            List<Vector3> path = new();
            RoadPoint pathRoadPoint = closestRoadPoint;
            float pathLength = 0f;
            while (pathRoadPoint != null)
            {
                path.Add(pathRoadPoint.Position);
                RoadPoint previousPoint;
                if (cameFrom.TryGetValue(pathRoadPoint, out previousPoint) && previousPoint != null)
                {
                    pathLength += Vector3.Distance(pathRoadPoint.Position, previousPoint.Position);
                }
                cameFrom.TryGetValue(pathRoadPoint, out pathRoadPoint);
            }

            PathData pathData = new(
                path.Reverse<Vector3>().ToArray(),
                pathLength);

            return pathData;
        }

        public RoadPoint GetClosestRoadPoint(Vector3 position)
        {
            if (_roadPoints == null || _roadPoints.Length == 0) return null;

            RoadPoint closestRoadPoint = null;
            float closestDistance = float.PositiveInfinity;

            for (int i = 0; i < _roadPoints.Length; i++)
            {
                var roadPoint = _roadPoints[i];

                float distance = Vector3.Distance(position, roadPoint.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestRoadPoint = roadPoint;
                }
            }

            return closestRoadPoint;
        }

        private void ConstructRoadGrid()
        {
            _roadPointGrid = new(_roadPointGridCellSize);
            for (int i = 0; i < _roadPoints.Length; i++)
            {
                _roadPointGrid.Insert(
                    _roadPoints[i].Position,
                    _roadPoints[i]);
            }

            _roadSpawnPointGrid = new(_roadSpawnPointGridCellSize);
            for (int i = 0; i < _roadSpawnPoints.Length; i++)
            {
                _roadSpawnPointGrid.Insert(
                    _roadSpawnPoints[i].transform.position,
                    _roadSpawnPoints[i]);
            }
        }

        public class PathData
        {
            public PathData(Vector3[] path, float length)
            {
                Path = path;
                Length = length;
            }

            public Vector3[] Path { get; }
            public float Length { get; }
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(RoadNetwork))]
    public class RoadNetworkEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RoadNetwork roadNetwork = (RoadNetwork)target;

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Construct Road Graph"))
            {
                roadNetwork.ConstructRoadGraph();
            }
        }
    }
#endif
}