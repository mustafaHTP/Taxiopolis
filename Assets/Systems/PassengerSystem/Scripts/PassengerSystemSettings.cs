using System;
using Game.Systems.PassengerSystem.Pooling;
using Game.Systems.SoundSystem;
using Game.Utils;
using UnityEngine;

namespace Game.Systems.PassengerSystem
{
    [CreateAssetMenu(fileName = "New Passenger System Settings", menuName = "Game/Systems/Passenger/Settings")]
    public class PassengerSystemSettings : ScriptableObject
    {
        [SerializeField] private CustomPlatform _customPlatform;
        [SerializeField] private int _passengerPoolSize;
        [SerializeField] private PassengerPoolProfile _passengerPoolProfile;
        [SerializeField]
        [Tooltip("Used for spatial hash grid to optimize passenger point queries. Half to full of the max spawn distance is a good start.")]
        private float _gridCellSize = 15;
        [SerializeField] private int _activePassengerInsideSphereLimit = 2;
        [SerializeField] private float _simInterval = 1f;
        [SerializeField]
        [Tooltip("Time factor per unit distance")]
        private float _tripDistanceTimeFactor = 0.1f;
        [SerializeField]
        [Tooltip("Min time for trip. Trip time cannot be shorter than this.")]
        private float _minTripTime = 10f;
        [SerializeField]
        [Tooltip("How much time added when passenger picked up. Based on pick up bonus time = Trip Time X Pick Up Bonus Time Factor")]
        [Range(0f, 1f)]
        private float _minPickUpBonusTimeFactor = 0.1f;
        [SerializeField]
        [Tooltip("How much time added when passenger picked up. Based on pick up bonus time = Trip Time X Pick Up Bonus Time Factor")]
        [Range(0f, 1f)]
        private float _maxPickUpBonusTimeFactor = 0.7f;
        [SerializeField]
        [Tooltip("Cost factor per unit distance")]
        private float _tripDistanceCostFactor = 2f;
        [SerializeField]
        [Tooltip("! Must be descending order by remaining time threshold")]
        private PassengerJobResultTier[] _passengerJobResultTiers;

        public float TripDistanceTimeFactor => _tripDistanceTimeFactor;
        public float TripDistanceCostFactor => _tripDistanceCostFactor;
        public PassengerJobResultTier[] PassengerJobResultTiers => _passengerJobResultTiers;
        public float MinTripTime => _minTripTime;
        public float MinPickUpBonusTimeFactor => _minPickUpBonusTimeFactor;
        public float MaxPickUpBonusTimeFactor => _maxPickUpBonusTimeFactor;
        public CustomPlatform CustomPlatform => _customPlatform;
        public int PassengerPoolSize => _passengerPoolSize;
        public int ActivePassengerInsideSphereLimit => _activePassengerInsideSphereLimit;
        public float SimInterval => _simInterval;
        public PassengerPoolProfile PassengerPoolProfile => _passengerPoolProfile;
        public float GridCellSize => _gridCellSize;

        private void OnValidate()
        {
            _tripDistanceTimeFactor = Mathf.Max(0f, _tripDistanceTimeFactor);
            _tripDistanceCostFactor = Mathf.Max(0f, _tripDistanceCostFactor);
            _minTripTime = Mathf.Max(0f, _minTripTime);
            _maxPickUpBonusTimeFactor = Mathf.Max(_minPickUpBonusTimeFactor, _maxPickUpBonusTimeFactor);

            if (_passengerJobResultTiers == null) return;

            for (int i = 1; i < _passengerJobResultTiers.Length; i++)
            {
                PassengerJobResultTier previous = _passengerJobResultTiers[i - 1];
                PassengerJobResultTier current = _passengerJobResultTiers[i];
                if (previous == null || current == null) continue;

                if (current.RemainingTimeRatioThreshold > previous.RemainingTimeRatioThreshold)
                {
                    Debug.LogWarning(
                        $"[{name}] Passenger job result tiers must be in descending order of " +
                        $"remaining time threshold (problem at element {i}).",
                        this);

                    break;
                }
            }
        }
    }

    [Serializable]
    public class PassengerJobResultTier
    {
        [SerializeField] private PassengerJobResultType _tierType;
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Represents the remaining time ratio threshold to qualify for this tier (0 to 1)")]
        private float _remainingTimeRatioThreshold;
        [SerializeField] private float _timeBonus;
        [SerializeField]
        [Tooltip("It is used for ui")]
        private string _coolName;
        [SerializeField] private SoundData _voiceOver;
        [SerializeField]
        [Tooltip("Color that represents that tier")]
        private Color _color;

        public PassengerJobResultType TierType => _tierType;
        public float RemainingTimeRatioThreshold => _remainingTimeRatioThreshold;
        public float TimeBonus => _timeBonus;
        public string CoolName => _coolName;
        public SoundData VoiceOver => _voiceOver;
        public Color Color => _color;
    }

    public enum PassengerJobResultType
    {
        None,
        Slow,
        Medium,
        Fast
    }
}
