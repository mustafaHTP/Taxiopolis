using System;
using System.Collections.Generic;
using Game.Systems.PassengerSystem.Passengers;
using Game.Utils;
using UnityEngine;

namespace Game.Systems.PassengerSystem
{
    public class PassengerJobHelper
    {
        private readonly PassengerSystemSettings _passengerSystemSettings;

        public PassengerJobHelper(PassengerSystemSettings passengerSystemSettings)
        {
            _passengerSystemSettings = passengerSystemSettings;
        }

        public PassengerJobResultTier EvaluatePassengerJobResultTier(float remainingPassengerTime, float passengerTripTime)
        {
            if (_passengerSystemSettings == null)
            {
                LogX.LogError($"{nameof(_passengerSystemSettings)} is null!");
                return null;
            }

            var tiers = _passengerSystemSettings.PassengerJobResultTiers;
            if (tiers == null || tiers.Length == 0)
            {
                LogX.LogError($"{nameof(_passengerSystemSettings.PassengerJobResultTiers)} is not set!");
                return null;
            }

            float normalizedRemainingTime = passengerTripTime > 0f
                ? Mathf.Clamp01(remainingPassengerTime / passengerTripTime)
                : 0f;

            for (int i = 0; i < tiers.Length; i++)
            {
                if (normalizedRemainingTime >= tiers[i].RemainingTimeRatioThreshold)
                {
                    return tiers[i];
                }
            }

            // Nothing matched: fall back to the lowest tier
            return tiers[tiers.Length - 1];
        }
        public PassengerJob CreateJob(Passenger passenger, PassengerWaitingPoint waitingPoint, PassengerDestinationPoint destinationPoint)
        {
            float tripDistance = Vector3.Distance(waitingPoint.Position, destinationPoint.Position);

            // Prevent trip time from being lower than min trip time
            float tripTime = Mathf.Max(
                tripDistance * _passengerSystemSettings.TripDistanceTimeFactor,
                _passengerSystemSettings.MinTripTime);

            float randomPickUpBonusTimeFactor = UnityEngine.Random.Range(
                _passengerSystemSettings.MinPickUpBonusTimeFactor,
                _passengerSystemSettings.MaxPickUpBonusTimeFactor);
            float pickUpBonusTime = tripTime * randomPickUpBonusTimeFactor;
            int fare = Mathf.CeilToInt(tripDistance * _passengerSystemSettings.TripDistanceCostFactor);

            var job = new PassengerJob(
                tripTime: tripTime,
                tripDistance: tripDistance,
                pickupBonusTime: pickUpBonusTime,
                fare: fare,
                passenger: passenger,
                waitingPoint: waitingPoint,
                destinationPoint: destinationPoint);

            return job;
        }
    }
}