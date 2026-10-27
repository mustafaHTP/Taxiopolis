using Game.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace Game.Common
{
    public class WaypointRoute : MonoBehaviour
    {
        private Waypoint[] _waypoints;

        public int RouteLength => _waypoints == null ? 0 : _waypoints.Length;

        private void Awake()
        {
            _waypoints = GetComponentsInChildren<Waypoint>();
            if (_waypoints == null)
            {
                LogX.LogError($"WaypointRoute {name} has no waypoints");

                return;
            }
        }

        public bool IsEndOfRoute(Waypoint waypoint)
        {
            if (_waypoints == null || _waypoints.Length == 0)
            {
                LogX.LogError($"WaypointRoute {name} has no waypoints");

                return false;
            }

            if (!_waypoints.Contains(waypoint))
            {
                LogX.LogError($"Waypoint {waypoint.name} is not part of the route {name}");

                return false;
            }

            int waypointIndex = Array.IndexOf(_waypoints, waypoint);

            return waypointIndex == _waypoints.Length - 1;
        }

        public Waypoint GetStartingWaypoint()
        {
            if (_waypoints == null || _waypoints.Length == 0)
            {
                LogX.LogError($"WaypointRoute {name} has no waypoints");

                return null;
            }

            return _waypoints[0];
        }

        public Waypoint GetNextWaypoint(Waypoint waypoint)
        {
            if (_waypoints == null || _waypoints.Length == 0)
            {
                LogX.LogError($"WaypointRoute {name} has no waypoints");

                return null;
            }

            if (!_waypoints.Contains(waypoint))
            {
                LogX.LogError($"Waypoint {waypoint.name} is not part of the route {name}");

                return null;
            }

            int currentIndex = Array.IndexOf(_waypoints, waypoint);
            int nextIndex = (currentIndex + 1) % _waypoints.Length;

            return _waypoints[nextIndex];
        }
    }
}
