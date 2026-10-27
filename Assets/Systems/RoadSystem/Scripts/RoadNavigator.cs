using Game.Systems.PassengerSystem;
using Game.Utils;
using UnityEngine;

namespace RoadSystem
{
    /// <summary>
    /// RoadNavigator is responsible for finding and navigating paths along the road network.
    /// It acts as guide for vehicles to traverse the roads.
    /// </summary>
    public class RoadNavigator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _isPathfindingEnabled = false;
        [SerializeField] private float _pathfindingInterval = 1.0f;
        [SerializeField] private bool _updatePathfindingContinuously = true;
        [SerializeField] private Transform _destinationPoint;
        [SerializeField] private RoadNetwork _roadNetwork;
        [SerializeField] private LineRenderer _lineRenderer;


        private float _nextPathfindingTime = 0f;
        private RoadNetwork.PathData _currentPathData;
        private Coroutine _pathFindingCoroutine;
        private PathStatus _pathStatus = PathStatus.NoPath;
        private CarPassengerHandler _carPassengerHandler;

        private enum PathStatus
        {
            NoPath,
            PathFound,
            PathInProgress
        }

        private void Awake()
        {
            _carPassengerHandler = GetComponent<CarPassengerHandler>();
        }

        private void Update()
        {
            if (_isPathfindingEnabled
                && _updatePathfindingContinuously
                && Time.time > _nextPathfindingTime
                && (_pathStatus == PathStatus.NoPath || _pathStatus == PathStatus.PathFound))
            {
                CalculatePath();

                _nextPathfindingTime = Time.time + _pathfindingInterval;
            }
        }

        private void OnEnable()
        {
            _carPassengerHandler.OnPassengerPickedUp += CarPassengerHandler_OnPassengerPickedUp;
            _carPassengerHandler.OnPassengerDroppedOff += CarPassengerHandler_OnPassengerDroppedOff;
        }


        private void OnDisable()
        {
            _carPassengerHandler.OnPassengerPickedUp -= CarPassengerHandler_OnPassengerPickedUp;
            _carPassengerHandler.OnPassengerDroppedOff -= CarPassengerHandler_OnPassengerDroppedOff;
        }

        private void OnPathFindingComplete(RoadNetwork.PathData pathData)
        {
            if (_pathStatus != PathStatus.PathInProgress)
            {
                LogX.LogWarning("Path is still in progress but some coroutine tries to finish it!");

                return;
            }

            if (pathData == null || pathData.Path.Length == 0)
            {
                LogX.LogWarning("No path found between the specified points.");

                _currentPathData = null;
                ClearPath();

                _pathStatus = PathStatus.NoPath;

                return;
            }

            _currentPathData = pathData;
            DrawPath(_currentPathData.Path);

            _pathStatus = PathStatus.PathFound;
        }

        public void CalculatePath()
        {
            if (_pathStatus == PathStatus.PathInProgress) return;

            if (_destinationPoint == null)
            {
                LogX.LogError("Destination point is not set.");

                return;
            }

            if (_pathFindingCoroutine != null)
            {
                StopCoroutine(_pathFindingCoroutine);
                _pathFindingCoroutine = null;
            }

            _pathStatus = PathStatus.PathInProgress;
            _pathFindingCoroutine = StartCoroutine(
                 _roadNetwork.FindPathAStarCoroutine(
                     transform.position,
                     _destinationPoint.position,
                     OnPathFindingComplete));
        }

        private void ClearPath()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 0;
            }
        }

        private void ClearDestinationPoint() => _destinationPoint = null;
        private void ActivatePathfinding() => _isPathfindingEnabled = true;
        private void DeactivatePathfinding() => _isPathfindingEnabled = false;

        private void DrawPath(Vector3[] path)
        {
            if (_lineRenderer == null)
            {
                LogX.LogWarning("LineRenderer is not assigned for path drawing.");

                return;
            }

            _lineRenderer.positionCount = path.Length;
            for (int i = 0; i < path.Length; i++)
            {
                var point = path[i];
                // Slightly above ground for visibility
                point.y += 0.2f;
                _lineRenderer.SetPosition(i, point);
            }
        }

        #region Handling Events

        private void CarPassengerHandler_OnPassengerDroppedOff(PassengerJob job)
        {
            ClearPath();
            ClearDestinationPoint();

            DeactivatePathfinding();
        }

        private void CarPassengerHandler_OnPassengerPickedUp(PassengerJob job)
        {
            if (_pathFindingCoroutine != null)
            {
                StopCoroutine(_pathFindingCoroutine);
                _pathFindingCoroutine = null;
            }

            ClearPath();
            ClearDestinationPoint();

            _destinationPoint = job.Passenger.DestinationPoint.transform;
            _pathStatus = PathStatus.NoPath;
            _currentPathData = null;

            ActivatePathfinding();
        }
        #endregion
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(RoadNavigator))]
    public class RoadNavigatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            RoadNavigator roadNavigator = (RoadNavigator)target;
            if (GUILayout.Button("Calculate Path"))
            {
                roadNavigator.CalculatePath();
            }
        }
    }
#endif
}
