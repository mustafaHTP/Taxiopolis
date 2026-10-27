using Game.Systems.EmoteSystem;
using Game.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Systems.PassengerSystem.Passengers.FSM
{
    public class AngryState : BaseState
    {
        // How far from the car's entry point a NavMesh position is searched.
        // A bounded value avoids snapping the passenger to a faraway part of the mesh.
        private const float NavMeshSampleDistance = 3f;

        public AngryState(Passenger passenger, string name) : base(passenger, name)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _passenger.PassengerMovementHandler.DisableAgent();
            _passenger.MeshesGameObject.SetActive(true);
            _passenger.transform.SetParent(null);

            PlaceNextToCar();

            _passenger.PassengerAnimationHandler.Play(_passenger.PassengerAnimationHandler.IdleAnimationHash);

            _passenger.EmoteController.SetEmote(EmoteType.FaceAngry);
            _passenger.EmoteController.Show();
        }

        public override void OnExit()
        {
            base.OnExit();

            _passenger.EmoteController.Hide();
        }

        private void PlaceNextToCar()
        {
            CarPassengerHandler car = _passenger.NotifyingCar;
            if (car == null || car.CarEntryPoints == null || car.CarEntryPoints.Length == 0)
            {
                // No entry point to use, so use the car itself (or stay where we are).
                LogX.LogWarning("Angry passenger has no car entry point to stand next to.");

                if (car != null)
                {
                    _passenger.transform.position = car.transform.position;
                }

                return;
            }

            Vector3 entryPosition = car.CarEntryPoints[0].position;

            _passenger.transform.position = NavMesh.SamplePosition(
                entryPosition,
                out NavMeshHit navMeshHit,
                NavMeshSampleDistance,
                NavMesh.AllAreas)
                ? navMeshHit.position
                : entryPosition;
        }
    }
}