using System;
using UnityEngine;

namespace RoadSystem.TrafficLightControl
{
    [Serializable]
    public class TrafficLightCycle
    {
        [Header("Lights Durations")]
        [SerializeField] private float _greenLightDuration = 10f;
        [SerializeField] private float _yellowLightDuration = 3f;

        [Header("Traffic Lights")]
        [SerializeField] private TrafficLight[] _trafficLights;

        public TrafficLight[] TrafficLights => _trafficLights;

        public float GreenLightDuration => _greenLightDuration;
        public float YellowLightDuration => _yellowLightDuration;
    }
}
