using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoadSystem
{
    /// <summary>
    /// RoadPoint represents a point on a road, which can be connected to other road points.
    /// It is meaningless without a parent Road component.
    /// </summary>
    public class RoadPoint : MonoBehaviour
    {
        [SerializeField]
        private Road _parent;
        [SerializeField]
        private List<RoadPoint> _connections = new();
        [SerializeField] private float _speedLimit = 5f;
        [SerializeField] private bool _isAllowedToDrive = true;

        public List<RoadPoint> Connections => _connections;
        public Road Parent
        {
            get
            {
                if (_parent == null)
                {
                    _parent = GetComponentInParent<Road>();
                    if (_parent == null)
                    {
#if UNITY_EDITOR
                        EditorGUIUtility.PingObject(this);
#endif
                        throw new NullReferenceException($"{name} does not have a parent Road component.");
                    }
                }

                return _parent;
            }
        }

        public List<RoadPoint> Neighbours
        {
            get
            {
                List<RoadPoint> neighbours = new(_connections);
                RoadPoint nextPoint = GetNext();

                if (nextPoint != null)
                {
                    neighbours.Add(nextPoint);
                }

                return neighbours;
            }
        }

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        public void ClearConnection() => _connections.Clear();

        public int ConnectionsCount => _connections.Count;

        public float SpeedLimit => _speedLimit;
        public bool IsAllowedToDrive { get => _isAllowedToDrive; set => _isAllowedToDrive = value; }

        public int GetIndex()
        {
            if (Parent == null)
            {
#if UNITY_EDITOR
                EditorGUIUtility.PingObject(this);
#endif

                throw new NullReferenceException($"{name}'s {nameof(Parent)} not set.");
            }

            return Parent.RoadPoints.IndexOf(this);
        }

        public RoadPoint GetNext()
        {
            if (Parent == null)
            {
#if UNITY_EDITOR
                EditorGUIUtility.PingObject(this);
#endif
                throw new NullReferenceException($"{name}'s {nameof(Parent)} not set.");
            }

            int index = GetIndex();
            if (index < 0 || index >= Parent.RoadPoints.Count)
            {
#if UNITY_EDITOR
                EditorGUIUtility.PingObject(this);
#endif
                throw new IndexOutOfRangeException($"Index {index} is out of range for road points in {Parent.name}.");
            }

            if (index == Parent.RoadPoints.Count - 1)
            {
                return null; // Last point has no next point
            }

            return Parent.RoadPoints[index + 1];
        }
    }
}
