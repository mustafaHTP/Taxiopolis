using Framework.Grid;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Systems.PedestrianSystem
{
    /// <summary>
    /// Finds all pedestrian points in the scene and indexes them in a spatial grid,
    /// so other systems can ask "which points are near this position?" cheaply.
    /// The grid can be drawn in the Scene view to tune the cell size.
    /// </summary>
    public class PedestrianPointRegistry
    {
        private SpatialHashGrid<PedestrianPoint> _pointGrid;
        private bool _isInitialized = false;

        public PedestrianPoint[] PedestrianPoints { get; private set; }

        public void Init(PedestrianSystemSettings pedestrianSystemSettings)
        {
            if (_isInitialized)
            {
                LogX.LogWarning("PedestrianPointRegistry has already been initialized.");

                return;
            }

            PedestrianPoints = GameObject.FindObjectsByType<PedestrianPoint>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            if (PedestrianPoints == null || PedestrianPoints.Length == 0)
            {
                LogX.LogError("No PedestrianPoint instances found in the scene.");

                return;
            }

            // build grid
            _pointGrid = new SpatialHashGrid<PedestrianPoint>(pedestrianSystemSettings.GridCellSize);
            for (int i = 0; i < PedestrianPoints.Length; i++)
            {
                _pointGrid.Insert(PedestrianPoints[i].transform.position, PedestrianPoints[i]);
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Fills <paramref name="results"/> (cleared first) with the points in the grid cells
        /// around <paramref name="center"/>. This is a coarse result: do the exact distance
        /// check yourself. Pass a reused list so the query does not allocate.
        /// </summary>
        public void QueryNearbyPoints(Vector3 center, float radius, List<PedestrianPoint> results)
        {
            results.Clear();

            if (!_isInitialized) return;

            _pointGrid.QueryRadius(center, radius, results);
        }
    }
}