using System;
using UnityEngine;

namespace Game.Systems.PassengerSystem
{
    public class PassengerZone : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private Color _debugZoneColor = Color.cyan;

        [Header("Settings")]
        [SerializeField] private string _id = Guid.NewGuid().ToString();

        private PassengerWaitingPoint[] _passengerWaitingPointsCache;
        private PassengerDestinationPoint[] _passengerDestinationPointsCache;

        public PassengerWaitingPoint[] PassengerWaitingPoints
        {
            get
            {
                return _passengerWaitingPointsCache ??= GetComponentsInChildren<PassengerWaitingPoint>();
            }
        }

        public PassengerDestinationPoint[] PassengerDestinationPoints
        {
            get
            {
                return _passengerDestinationPointsCache ??= GetComponentsInChildren<PassengerDestinationPoint>();
            }
        }

        public string ZoneId => _id;

        public Color DebugZoneColor => _debugZoneColor;
    }
}
