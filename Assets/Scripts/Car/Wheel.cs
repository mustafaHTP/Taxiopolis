using System;
using UnityEngine;

namespace Game.Car
{
    [Serializable]
    public class Wheel
    {
        [SerializeField] private Transform _wheelPoint;
        [SerializeField] private Transform _wheelMesh;

        private WheelState _wheelState = new();

        public Transform WheelPoint => _wheelPoint;
        public Transform WheelMesh => _wheelMesh;

        public WheelState WheelState => _wheelState;
    }
}
