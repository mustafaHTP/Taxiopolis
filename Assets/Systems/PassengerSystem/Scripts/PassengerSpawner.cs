using Game.Systems.PassengerSystem.Passengers;
using Game.Systems.PassengerSystem.Pooling;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Systems.PassengerSystem
{
    /// <summary>
    /// Spawns passengers at the waiting points it is given (with a random destination in
    /// ANOTHER zone and a job), and despawns the passengers it is given.
    /// It does not decide where or how many. That is the sim sphere's job.
    /// </summary>
    public class PassengerSpawner
    {
        // Passengers the simulation manages (waiting, chasing, dropped off).
        // Passengers riding in the player's car are NOT in this list on purpose,
        // so the sim sphere never counts them and can never despawn them.
        private readonly List<Passenger> _activePassengers = new();
        private readonly HashSet<Passenger> _ridingPassengers = new();

        // Which waiting points currently have a passenger, and which point each passenger came from.
        // Lets us answer "is this point taken?" in O(1) instead of searching the active list.
        private readonly HashSet<PassengerWaitingPoint> _occupiedWaitingPoints = new();
        private readonly Dictionary<Passenger, PassengerWaitingPoint> _waitingPointByPassenger = new();

        private Transform _spawnRoot;
        private PassengerPoolProfile _passengerPoolProfile;
        private PassengerPointRegistry _passengerPointRegistry;
        private PassengerPool _passengerPool;
        private PassengerJobHelper _passengerJobHelper;
        private bool _isInitialized = false;

        public IReadOnlyList<Passenger> ActivePassengers => _activePassengers;

        public void Init(
            PassengerSystemSettings passengerSystemSettings, 
            PassengerJobHelper passengerJobHelper, 
            PassengerPointRegistry passengerPointRegistry,
            Transform spawnRoot)
        {
            if (_isInitialized)
            {
                LogX.LogWarning("PassengerSpawner has already been initialized.");

                return;
            }

            if (passengerSystemSettings == null)
            {
                LogX.LogError("Passenger system settings is null!");

                return;
            }

            if (passengerJobHelper == null)
            {
                LogX.LogError("PassengerJobHelper is null!");

                return;
            }

            _spawnRoot = spawnRoot;
            _passengerPointRegistry = passengerPointRegistry;
            _passengerPoolProfile = passengerSystemSettings.PassengerPoolProfile;

            _passengerJobHelper = passengerJobHelper;

            _passengerPool = new PassengerPool(
                _passengerPoolProfile,
                passengerSystemSettings.PassengerPoolSize,
                _spawnRoot);

            _passengerPool.Init();

            _isInitialized = true;
        }

        public bool IsWaitingPointOccupied(PassengerWaitingPoint point)
        {
            return _occupiedWaitingPoints.Contains(point);
        }

        /// <summary>
        /// Called when the car accepted the passenger. From now on the simulation ignores it:
        /// it is not counted, it cannot be despawned, and its waiting point becomes free again.
        /// </summary>
        public void ReleaseFromSimulation(Passenger passenger)
        {
            if (!_isInitialized || passenger == null) return;

            if (!_activePassengers.Remove(passenger))
            {
                LogX.LogWarning("Cannot release a passenger that is not tracked by the simulation.");

                return;
            }

            _ridingPassengers.Add(passenger);
            FreeWaitingPoint(passenger);
        }

        /// <summary>
        /// Called when the passenger left the car. The simulation manages it again,
        /// so it is despawned by distance once the player has moved away.
        /// The passenger must no longer be parented to the car at this point.
        /// </summary>
        public void ReturnToSimulation(Passenger passenger)
        {
            if (!_isInitialized || passenger == null) return;

            if (!_ridingPassengers.Remove(passenger))
            {
                LogX.LogWarning("Cannot return a passenger that was not released from the simulation.");

                return;
            }

            _activePassengers.Add(passenger);
        }

        /// <summary>
        /// Tries to spawn passengers at the given points, in list order, until
        /// <paramref name="maxCount"/> passengers were actually spawned.
        /// Points that fail (occupied, no zone, no NavMesh...) do not use up the budget.
        /// </summary>
        public void SpawnPassengers(IReadOnlyList<PassengerWaitingPoint> candidateWaitingPoints, int maxCount)
        {
            if (!_isInitialized) return;
            if (candidateWaitingPoints == null || candidateWaitingPoints.Count == 0) return;
            if (maxCount <= 0) return;

            int spawned = 0;
            for (int i = 0; i < candidateWaitingPoints.Count && spawned < maxCount; i++)
            {
                SpawnResult result = TrySpawnPassenger(candidateWaitingPoints[i]);

                if (result == SpawnResult.Spawned)
                {
                    spawned++;
                }
                else if (result == SpawnResult.PoolExhausted)
                {
                    // No free passenger, so every remaining point would fail the same way.
                    LogX.LogWarning("Passenger pool is exhausted.");

                    break;
                }
            }
        }

        /// <summary>Tries to spawn one passenger at every given point.</summary>
        public void SpawnPassengers(IReadOnlyList<PassengerWaitingPoint> candidateWaitingPoints)
        {
            SpawnPassengers(candidateWaitingPoints, int.MaxValue);
        }

        public bool SpawnPassenger(PassengerWaitingPoint candidateWaitingPoint)
        {
            if (!_isInitialized) return false;

            return TrySpawnPassenger(candidateWaitingPoint) == SpawnResult.Spawned;
        }

        public void DespawnPassengers(IReadOnlyList<Passenger> passengers)
        {
            if (!_isInitialized) return;
            if (passengers == null || passengers.Count == 0) return;

            for (int i = 0; i < passengers.Count; i++)
            {
                DespawnPassenger(passengers[i]);
            }
        }

        public void DespawnPassenger(Passenger passenger)
        {
            if (!_isInitialized) return;

            if (passenger == null)
            {
                LogX.LogWarning("Cannot despawn a null passenger.");

                return;
            }

            // Works for tracked passengers and for riders (for example on scene teardown).
            bool wasTracked = _activePassengers.Remove(passenger);
            bool wasRiding = !wasTracked && _ridingPassengers.Remove(passenger);
            if (!wasTracked && !wasRiding)
            {
                LogX.LogWarning($"Passenger is not managed by {nameof(PassengerSpawner)}");

                return;
            }

            // Free the waiting point BEFORE returning to the pool, because the pool
            // may reset the passenger.
            FreeWaitingPoint(passenger);

            _passengerPool.Return(passenger);
        }

        private void FreeWaitingPoint(Passenger passenger)
        {
            if (_waitingPointByPassenger.TryGetValue(passenger, out PassengerWaitingPoint waitingPoint))
            {
                _occupiedWaitingPoints.Remove(waitingPoint);
                _waitingPointByPassenger.Remove(passenger);
            }
        }

        private enum SpawnResult
        {
            Spawned,
            Skipped,
            PoolExhausted
        }

        private SpawnResult TrySpawnPassenger(PassengerWaitingPoint waitingPoint)
        {
            if (waitingPoint == null) return SpawnResult.Skipped;

            if (!waitingPoint.HasZone)
            {
                LogX.LogWarning("PassengerPoint does not have an associated PassengerZone.", waitingPoint);

                return SpawnResult.Skipped;
            }

            // Do not spawn two passengers at the same place.
            if (_occupiedWaitingPoints.Contains(waitingPoint)) return SpawnResult.Skipped;

            // Check everything that can fail BEFORE taking a passenger from the pool,
            // otherwise a failed spawn would leave an active passenger that is never tracked.
            if (!NavMesh.SamplePosition(
                waitingPoint.Position,
                out NavMeshHit hit,
                float.PositiveInfinity,
                NavMesh.AllAreas))
            {
                LogX.LogWarning("No NavMesh position found near the waiting point.", waitingPoint);

                return SpawnResult.Skipped;
            }

            PassengerDestinationPoint destinationPoint = PickDestinationPoint(waitingPoint);
            if (destinationPoint == null) return SpawnResult.Skipped;

            Passenger passenger = _passengerPool.Get();
            if (passenger == null) return SpawnResult.PoolExhausted;

            passenger.transform.SetPositionAndRotation(hit.position, waitingPoint.Rotation);

            PassengerJob passengerJob = _passengerJobHelper.CreateJob(passenger, waitingPoint, destinationPoint);
            passenger.AssignJob(passengerJob);

            _activePassengers.Add(passenger);
            _occupiedWaitingPoints.Add(waitingPoint);
            _waitingPointByPassenger[passenger] = waitingPoint;

            LogX.LogInfo($"Spawned passenger at {waitingPoint.name} heading to {destinationPoint.name}");
            LogX.LogInfo($"Trip Distance: {passenger.Job.TripDistance}, Estimated Trip Time: {passenger.Job.TripTime}");

            return SpawnResult.Spawned;
        }

        /// <summary>
        /// Picks a random destination point in a random zone that is DIFFERENT from the
        /// waiting point's zone. No allocations: a random index is drawn from the other zones.
        /// Zones are placed in the scene so trips are meaningful, so no distance filtering is needed.
        /// </summary>
        private PassengerDestinationPoint PickDestinationPoint(PassengerWaitingPoint waitingPoint)
        {
            PassengerZone[] zones = _passengerPointRegistry.PassengerZones;
            if (zones == null || zones.Length == 0)
            {
                LogX.LogError("No PassengerZones found in the scene.");

                return null;
            }

            int currentZoneIndex = System.Array.IndexOf(zones, waitingPoint.Zone);
            if (currentZoneIndex < 0)
            {
                LogX.LogError("PassengerPoint's zone is not recognized by PassengerSpawner.", waitingPoint);

                return null;
            }

            if (zones.Length < 2)
            {
                LogX.LogWarning("No other zone available as a destination for the passenger.");

                return null;
            }

            // Draw from (zones - 1) values and skip over the current zone's index.
            int zoneIndex = Random.Range(0, zones.Length - 1);
            if (zoneIndex >= currentZoneIndex) zoneIndex++;

            PassengerZone destinationZone = zones[zoneIndex];
            PassengerDestinationPoint[] destinationPoints = destinationZone.PassengerDestinationPoints;
            if (destinationPoints == null || destinationPoints.Length == 0)
            {
                LogX.LogWarning("Destination Zone has no destination points.", destinationZone);

                return null;
            }

            return destinationPoints[Random.Range(0, destinationPoints.Length)];
        }
    }
}