using UnityEngine;

namespace RoadSystem.TrafficLightControl
{
    public class TrafficLight : MonoBehaviour
    {
        [SerializeField] private Material _greenLightMaterial;
        [SerializeField] private Material _yellowLightMaterial;
        [SerializeField] private Material _redLightMaterial;
        [Space(5)]
        [SerializeField] private RoadPoint _connectedRoadPoint;

        private TrafficLightState _trafficLightState = TrafficLightState.Red;

        public RoadPoint ConnectedRoadPoint { get => _connectedRoadPoint; set => _connectedRoadPoint = value; }
        public TrafficLightState TrafficLightState => _trafficLightState;

        public void ChangeTrafficLightState(TrafficLightState trafficLightState)
        {
            _trafficLightState = trafficLightState;

            switch (TrafficLightState)
            {
                case TrafficLightState.Green:
                    //GetComponent<Renderer>().material = _greenLightMaterial;
                    _connectedRoadPoint.IsAllowedToDrive = true;
                    break;
                case TrafficLightState.Yellow:
                    //GetComponent<Renderer>().material = _yellowLightMaterial;
                    _connectedRoadPoint.IsAllowedToDrive = false;
                    break;
                case TrafficLightState.Red:
                    //GetComponent<Renderer>().material = _redLightMaterial;
                    _connectedRoadPoint.IsAllowedToDrive = false;
                    break;
            }
        }
    }
}
