using Framework.Grid;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Systems.PassengerSystem
{
    /// <summary>
    /// Finds all passenger zones in the scene, gathers their waiting points, and indexes the
    /// points in a spatial grid so other systems can ask "which points are near this position?"
    /// cheaply. The grid can be drawn in the Scene view to tune the cell size.
    /// </summary>
    public class PassengerPointRegistry
    {
        private PassengerZone[] _passengerZones;
        private IReadOnlyList<PassengerWaitingPoint> _passengerPoints;
        private SpatialHashGrid<PassengerWaitingPoint> _pointGrid;
        private bool _isInitialized = false;

        public PassengerZone[] PassengerZones => _passengerZones;

        public void Init(PassengerSystemSettings passengerSystemSettings)
        {
            if (_isInitialized)
            {
                LogX.LogWarning("PassengerPointRegistry has already been initialized.");

                return;
            }

            _passengerZones = GameObject.FindObjectsByType<PassengerZone>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            if (_passengerZones.Length == 0)
            {
                LogX.LogError("No PassengerZones found in the scene.");

                return;
            }

            // For consistency, gather all waiting points from the found PassengerZones.
            // Do NOT search the entire scene for waiting points: this makes certain that
            // each waiting point belongs to a PassengerZone.
            List<PassengerWaitingPoint> passengerPoints = new();
            for (int i = 0; i < _passengerZones.Length; i++)
            {
                PassengerZone zone = _passengerZones[i];
                if (zone.PassengerWaitingPoints == null) continue;

                foreach (PassengerWaitingPoint point in zone.PassengerWaitingPoints)
                {
                    if (point == null)
                    {
                        LogX.LogWarning("A PassengerZone has an empty waiting point entry.", zone);

                        continue;
                    }

                    // A point finds its zone through the hierarchy (GetComponentInParent).
                    // If it is listed in a zone it is not a child of, spawning would use the wrong zone.
                    if (!point.HasZone || point.Zone != zone)
                    {
                        LogX.LogWarning("Waiting point is listed in a zone that is not its parent zone.", point);
                    }

                    passengerPoints.Add(point);
                }
            }

            _passengerPoints = passengerPoints;

            // build grid
            _pointGrid = new SpatialHashGrid<PassengerWaitingPoint>(passengerSystemSettings.GridCellSize);
            for (int i = 0; i < _passengerPoints.Count; i++)
            {
                _pointGrid.Insert(_passengerPoints[i].transform.position, _passengerPoints[i]);
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Fills <paramref name="results"/> (cleared first) with the points in the grid cells
        /// around <paramref name="center"/>. This is a coarse result: do the exact distance
        /// check yourself. Pass a reused list so the query does not allocate.
        /// </summary>
        public void QueryNearbyPoints(Vector3 center, float radius, List<PassengerWaitingPoint> results)
        {
            results.Clear();

            if (_pointGrid == null) return;

            _pointGrid.QueryRadius(center, radius, results);
        }
    }
}